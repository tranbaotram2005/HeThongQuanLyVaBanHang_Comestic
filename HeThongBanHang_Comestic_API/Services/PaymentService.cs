
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.DTOs.Payment;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public PaymentService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // =====================================================
        // 1. KHÁCH HÀNG XEM THANH TOÁN CỦA ĐƠN MÌNH
        // =====================================================
        public async Task<List<PaymentResponseDTO>> GetMyPaymentsAsync(
            int userId,
            int orderId)
        {
            // Tìm khách hàng theo UserId
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.Status == 1);

            if (customer == null)
                return new List<PaymentResponseDTO>();

            // Kiểm tra đơn hàng thuộc khách hàng
            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.CustomerId == customer.Id);

            if (order == null)
                return new List<PaymentResponseDTO>();

            // Lấy danh sách thanh toán
            var payments = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<PaymentResponseDTO>>(payments);
        }

        // =====================================================
        // 2. ADMIN, EMPLOYEE XEM THANH TOÁN THEO ĐƠN HÀNG
        // =====================================================
        public async Task<List<PaymentResponseDTO>>
            GetPaymentsByOrderIdAsync(int orderId)
        {
            // Kiểm tra đơn hàng tồn tại
            var orderExists = await _context.Orders
                .AnyAsync(o => o.Id == orderId);

            if (!orderExists)
                return new List<PaymentResponseDTO>();

            // Lấy thanh toán của đơn
            var payments = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<PaymentResponseDTO>>(payments);
        }

        // =====================================================
        // 3. ADMIN, EMPLOYEE XEM TẤT CẢ THANH TOÁN
        // =====================================================
        public async Task<List<PaymentResponseDTO>> GetAllPaymentsAsync()
        {
            var payments = await _context.Payments
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<PaymentResponseDTO>>(payments);
        }

        // =====================================================
        // 4. ADMIN, EMPLOYEE XEM CHI TIẾT THANH TOÁN
        // =====================================================
        public async Task<PaymentResponseDTO?> GetPaymentByIdAsync(
            int paymentId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return null;

            return _mapper.Map<PaymentResponseDTO>(payment);
        }

        // =====================================================
        // 5. TẠO THANH TOÁN
        // =====================================================
        public async Task<PaymentResponseDTO?> CreatePaymentAsync(
            PaymentCreateDTO dto,
            int userId)
        {
            // Kiểm tra đơn hàng
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");

            // Không tạo thanh toán cho đơn đã hủy
            if (order.Status == 3)
                throw new Exception("Đơn hàng đã hủy, không thể thanh toán.");

            // Kiểm tra số tiền
            if (dto.Amount <= 0)
                throw new Exception("Số tiền thanh toán phải lớn hơn 0.");

            // Tính tổng tiền đơn hàng
            decimal totalAmount = order.OrderDetails.Sum(d =>
                d.LineTotal ?? (d.UnitPrice * d.Quantity))
                + (order.Freight ?? 0);

            // Kiểm tra số tiền không vượt quá tổng đơn
            if (dto.Amount > totalAmount)
                throw new Exception("Số tiền thanh toán vượt quá tổng tiền đơn hàng.");

            // Kiểm tra phương thức thanh toán
            var validMethods = new[]
            {
                "Chuyển khoản",
                "Momo",
                "Tiền mặt"
            };

            if (!validMethods.Contains(dto.Method))
                throw new Exception("Phương thức thanh toán không hợp lệ.");

            // Không tạo thêm thanh toán nếu đơn đã được thanh toán
            bool alreadyPaid = order.Payments.Any(p => p.Status == 1);

            if (alreadyPaid)
                throw new Exception("Đơn hàng đã được thanh toán.");

            // Tạo Payment
            var payment = new Payment
            {
                OrderId = dto.OrderId,
                Amount = dto.Amount,
                Method = dto.Method,
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
                Notes = dto.Notes,
                Status = 0 // Chưa thanh toán
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return _mapper.Map<PaymentResponseDTO>(payment);
        }

        // =====================================================
        // 6. CẬP NHẬT TRẠNG THÁI THANH TOÁN
        // =====================================================
        public async Task<bool> UpdatePaymentStatusAsync(
            int paymentId,
            PaymentUpdateStatusDTO dto,
            int userId)
        {
            // Trạng thái hợp lệ: 0, 1, 2, 3
            if (dto.Status < 0 || dto.Status > 3)
                throw new Exception("Trạng thái thanh toán không hợp lệ.");

            // Tìm thanh toán
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                return false;

            // Không cho cập nhật thanh toán đã hoàn tiền
            if (payment.Status == 3 && dto.Status != 3)
                throw new Exception("Thanh toán đã hoàn tiền, không thể thay đổi.");

            // Không cho thay đổi thanh toán đã thất bại thành đã thanh toán
            if (payment.Status == 2 && dto.Status == 1)
                throw new Exception("Thanh toán đã thất bại.");

            payment.Status = dto.Status;
            payment.UpdatedAt = DateTime.Now;
            payment.UpdatedBy = userId;

            if (!string.IsNullOrWhiteSpace(dto.Notes))
            {
                payment.Notes = dto.Notes;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        // =====================================================
        // 7. XỬ LÝ WEBHOOK SEPAY
        // =====================================================
        public async Task<bool> ProcessSePayWebhookAsync(SePayWebhookDTO dto)
        {
            Console.WriteLine("========== SEPAY WEBHOOK DEBUG ==========");
            Console.WriteLine($"TransferType: [{dto.TransferType}]");
            Console.WriteLine($"Content: [{dto.Content}]");
            Console.WriteLine($"TransferAmount: [{dto.TransferAmount}]");
            Console.WriteLine($"ReferenceCode: [{dto.ReferenceCode}]");

            // 1. Kiểm tra tiền vào
            if (!string.Equals(
                dto.TransferType,
                "in",
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("LOI: TransferType khong phai in");
                return false;
            }

            // 2. Kiểm tra nội dung
            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                Console.WriteLine("LOI: Content bi rong");
                return false;
            }

            // 3. Lấy OrderId từ nội dung chuyển khoản
            var match = System.Text.RegularExpressions.Regex.Match(
                dto.Content,
                @"THANHTOAN\s+(\d+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                Console.WriteLine("LOI: Khong tim thay OrderId trong Content");
                return false;
            }

            if (!int.TryParse(match.Groups[1].Value, out int orderId))
            {
                Console.WriteLine("LOI: OrderId khong hop le");
                return false;
            }

            Console.WriteLine($"OrderId tim duoc: {orderId}");

            // 4. Kiểm tra số tiền
            if (dto.TransferAmount <= 0)
            {
                Console.WriteLine("LOI: TransferAmount <= 0");
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 5. Tìm đơn hàng
                var order = await _context.Orders
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    Console.WriteLine($"LOI: Khong tim thay Order ID {orderId}");
                    return false;
                }

                Console.WriteLine($"Order.Status: {order.Status}");

                // 6. Kiểm tra đơn đã hủy
                if (order.Status == 3)
                {
                    Console.WriteLine("LOI: Don hang da huy");
                    return false;
                }

                // 7. Kiểm tra trạng thái đơn
                if (order.Status != 0)
                {
                    Console.WriteLine("LOI: Order.Status khong phai 0");
                    return false;
                }

                // 8. Tìm Payment
                var payment = order.Payments.FirstOrDefault(p =>
                    p.Method == "Chuyển khoản" &&
                    p.Status == 0);

                if (payment == null)
                {
                    Console.WriteLine("LOI: Khong tim thay Payment chua thanh toan");
                    foreach (var p in order.Payments)
                    {
                        Console.WriteLine(
                            $"PaymentId={p.Id}, Method=[{p.Method}], Status={p.Status}, Amount={p.Amount}");
                    }

                    return false;
                }

                Console.WriteLine($"Payment.Amount: {payment.Amount}");

                // 9. Kiểm tra số tiền
                if (dto.TransferAmount != payment.Amount)
                {
                    Console.WriteLine(
                        $"LOI: So tien khong khop. Webhook={dto.TransferAmount}, Payment={payment.Amount}");
                    return false;
                }

                // 10. Cập nhật thanh toán
                payment.Status = 1;
                payment.UpdatedAt = DateTime.Now;
                payment.Notes =
                    $"Thanh toán SePay thành công. Mã giao dịch: {dto.ReferenceCode}";

                // 11. Cập nhật đơn hàng
                order.Status = 1;
                order.UpdatedAt = DateTime.Now;

                // 12. Lưu database
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                Console.WriteLine("SUCCESS: Da cap nhat Order va Payment");
                Console.WriteLine("=======================================");

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine("EXCEPTION SEPAY:");
                Console.WriteLine(ex.ToString());

                throw;
            }
        }
    }
}
