using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class OrderService : IOrderService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public OrderService(
    WebApiGtEcommerceContext context,
    IMapper mapper,
    IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        // Tạo đơn hàng từ toàn bộ giỏ hàng của khách
        public async Task<OrderResponseDTO?> CreateOrderAsync(OrderCreateDTO dto,int userId)
        {
            // 1. Tìm khách hàng dựa trên UserId từ JWT
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.Status == 1);

            if (customer == null)
                throw new Exception("Không tìm thấy thông tin khách hàng.");

            // 2. Tìm giỏ hàng của khách
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                throw new Exception("Giỏ hàng trống.");

            // 3. Kiểm tra phương thức thanh toán
            var validPaymentMethods = new[]
            {
                "Chuyển khoản",
                "Momo",
                "Tiền mặt"
            };

            if (!validPaymentMethods.Contains(dto.PaymentMethod))
                throw new Exception("Phương thức thanh toán không hợp lệ.");

            // 4. Tìm đơn vị vận chuyển đang hoạt động
            var shipper = await _context.Shippers
                .FirstOrDefaultAsync(s => s.Status == 1);

            if (shipper == null)
                throw new Exception("Không có đơn vị vận chuyển hoạt động.");

            // 5. Bắt đầu transaction
            using var transaction = await _context.Database
                .BeginTransactionAsync();

            try
            {
                // 6. Kiểm tra tồn kho từng sản phẩm trong giỏ
                foreach (var item in cart.CartItems)
                {
                    var variant = item.ProductVariant;

                    if (variant == null || variant.Status != 1)
                        throw new Exception(
                            $"Phiên bản sản phẩm ID {item.ProductVariantId} không còn hoạt động.");

                    if (variant.Product == null ||
                        variant.Product.Status != 1)
                        throw new Exception(
                            $"Sản phẩm {variant.Name} không còn kinh doanh.");

                    if (item.Quantity <= 0)
                        throw new Exception("Số lượng sản phẩm không hợp lệ.");

                    // Lấy tổng tồn kho của các kho đang hoạt động
                    var availableStock = await _context.Inventories
                        .Where(i =>
                            i.VariantId == variant.Id &&
                            i.Status == 1)
                        .SumAsync(i => i.Quantity ?? 0);

                    if (item.Quantity > availableStock)
                    {
                        throw new Exception(
                            $"Sản phẩm {variant.Name} không đủ tồn kho. " +
                            $"Hiện còn {availableStock}.");
                    }
                }

                // 7. Tạo Order
                var order = new Order
                {
                    CustomerId = customer.Id,
                    OrderDate = DateTime.Now,
                    Freight = 0,
                    Receive = "Giao tận nơi",
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    Notes = dto.Notes,
                    Status = 0
                };

                _context.Orders.Add(order);

                // Lưu Order trước để lấy OrderId
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;

                // 8. Tạo OrderDetail và trừ tồn kho
                foreach (var item in cart.CartItems)
                {
                    var variant = item.ProductVariant;
                    var product = variant.Product;

                    if (variant.Price == null || variant.Price <= 0)
                    {
                        throw new Exception($"Sản phẩm {variant.Name} chưa có giá bán hợp lệ.");
                    }

                    decimal unitPrice = variant.Price.Value;
                    decimal lineTotal = unitPrice * item.Quantity;

                    totalAmount += lineTotal;

                    // Tạo chi tiết đơn hàng
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        ProductVariantId = variant.Id,
                        UnitPrice = unitPrice,
                        Quantity = (short)item.Quantity,
                        ProductName = product.ProductName,
                        Sku = variant.Sku,
                        VariantInfo = variant.Name,
                        LineTotal = lineTotal,
                        Discount = 0,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                        Status = 1
                    };

                    _context.OrderDetails.Add(orderDetail);

                    // Lấy các kho còn tồn của biến thể
                    var inventories = await _context.Inventories
                        .Where(i =>
                            i.VariantId == variant.Id &&
                            i.Status == 1 &&
                            (i.Quantity ?? 0) > 0)
                        .OrderBy(i => i.Id)
                        .ToListAsync();

                    // Trừ kho lần lượt từng Inventory
                    int remainingQuantity = item.Quantity;

                    foreach (var inventory in inventories)
                    {
                        if (remainingQuantity <= 0)
                            break;

                        int currentQuantity = inventory.Quantity ?? 0;

                        int quantityToDeduct = Math.Min(
                            currentQuantity,
                            remainingQuantity);

                        inventory.Quantity =
                            currentQuantity - quantityToDeduct;

                        inventory.UpdatedAt = DateTime.Now;
                        inventory.UpdatedBy = userId;

                        remainingQuantity -= quantityToDeduct;
                    }

                    if (remainingQuantity > 0)
                    {
                        throw new Exception(
                            $"Không thể trừ đủ tồn kho cho {variant.Name}.");
                    }

                    // Đồng bộ ProductVariant.StockQuantity
                    variant.StockQuantity = inventories
                        .Where(i => i.Status == 1)
                        .Sum(i => i.Quantity ?? 0);

                    variant.UpdatedAt = DateTime.Now;
                    variant.UpdatedBy = userId;
                }

                // 9. Tạo Payment
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = totalAmount,
                    Method = dto.PaymentMethod,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    Notes = "Thanh toán đơn hàng",
                    Status = 0
                };

                _context.Payments.Add(payment);

                // 10. Tạo ShipInfo
                var shipInfo = new ShipInfo
                {
                    OrderId = order.Id,
                    ShipperId = shipper.Id,

                    CountryId = dto.CountryId,
                    ProvinceId = dto.ProvinceId,
                    WardId = dto.WardId,
                    TownId = dto.TownId,
                    AddressId = dto.AddressId,

                    ShipName = dto.ShipName,
                    ShipPhone = dto.ShipPhone,
                    ShipAddress = dto.ShipAddress,
                    ShipCity = dto.ShipCity,
                    ShipRegion = dto.ShipRegion,
                    ShipPostalCode = dto.ShipPostalCode,
                    ShipCountry = dto.ShipCountry,

                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    Notes = "Đơn hàng giao tiêu chuẩn",
                    Status = 1
                };

                _context.ShipInfos.Add(shipInfo);

                // 11. Xóa các sản phẩm đã đặt khỏi giỏ hàng
                _context.CartItems.RemoveRange(cart.CartItems);

                cart.UpdatedAt = DateTime.Now;

                // 12. Lưu toàn bộ thay đổi
                await _context.SaveChangesAsync();

                // 13. Commit transaction
                await transaction.CommitAsync();

                // 14. Trả dữ liệu đơn hàng
                var result = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .Include(o => o.Payments)
                    .Include(o => o.ShipInfos)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (result == null)
                    return null;

                var response = _mapper.Map<OrderResponseDTO>(result);

                response.TotalAmount = totalAmount;

                // Nếu khách chọn chuyển khoản
                if (dto.PaymentMethod == "Chuyển khoản")
                {
                    // Nội dung chuyển khoản dựa trên mã đơn hàng
                    response.TransferContent = $"THANHTOAN {result.Id}";

                    // Lấy thông tin ngân hàng từ appsettings.json
                    response.BankName = _configuration["BankTransfer:BankName"];

                    response.BankAccount = _configuration["BankTransfer:AccountNumber"];
                }

                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Lấy danh sách đơn hàng của khách hàng đang đăng nhập
        public async Task<List<OrderResponseDTO>> GetMyOrdersAsync(int userId)
        {
            // 1. Tìm Customer dựa trên UserId từ JWT
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == 1);

            // Nếu không tìm thấy khách hàng thì trả danh sách rỗng
            if (customer == null)
                return new List<OrderResponseDTO>();

            // 2. Lấy danh sách đơn hàng của khách hàng (gồm tự đặt và nhân viên đặt dùm)
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customer.Id)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.CreatedByNavigation)
                .Include(o => o.UpdatedByNavigation)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Include(o => o.ShipInfos)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapSingleOrder).ToList();
        }

        // Lấy chi tiết một đơn hàng của khách hàng đang đăng nhập
        public async Task<OrderResponseDTO?> GetMyOrderByIdAsync(
            int orderId,
            int userId)
        {
            // 1. Tìm khách hàng dựa trên UserId từ JWT
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.Status == 1);

            // Không tìm thấy khách hàng
            if (customer == null)
                return null;

            // 2. Tìm đơn hàng thuộc khách hàng đó
            var order = await _context.Orders
                .Where(o =>
                    o.Id == orderId &&
                    o.CustomerId == customer.Id)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.CreatedByNavigation)
                .Include(o => o.UpdatedByNavigation)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Include(o => o.ShipInfos)
                .FirstOrDefaultAsync();

            return order == null ? null : MapSingleOrder(order);
        }

        // Lấy danh sách đơn hàng do nhân viên đang đăng nhập đặt dùm khách
        public async Task<List<OrderResponseDTO>> GetOrdersByEmployeeAsync(int userId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Status == 1);

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.CreatedByNavigation)
                .Include(o => o.UpdatedByNavigation)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Include(o => o.ShipInfos)
                .Where(o =>
                    (employee != null && o.EmployeeId == employee.Id) ||
                    (o.CreatedBy == userId && (o.Customer == null || o.Customer.UserId != userId)))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapSingleOrder).ToList();
        }

        // Lấy danh sách tất cả đơn hàng (Admin, Employee)
        public async Task<List<OrderResponseDTO>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.CreatedByNavigation)
                .Include(o => o.UpdatedByNavigation)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Include(o => o.ShipInfos)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapSingleOrder).ToList();
        }

        // ============================================================
        // 5. Lấy chi tiết đơn hàng (Admin, Employee)
        // ============================================================
        public async Task<OrderResponseDTO?> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.CreatedByNavigation)
                .Include(o => o.UpdatedByNavigation)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Include(o => o.ShipInfos)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return order == null ? null : MapSingleOrder(order);
        }

        public async Task<List<CustomerSelectDTO>> GetCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c => c.Status == 1)
                .OrderBy(c => c.ContactName)
                .ThenBy(c => c.Id)
                .Select(c => new CustomerSelectDTO
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    ContactName = c.ContactName,
                    Phone = c.Phone,
                    Email = c.Email
                })
                .ToListAsync();
        }

        public async Task<OrderResponseDTO?> CreateOrderByEmployeeAsync(
            EmployeeOrderCreateDTO dto,
            int employeeUserId)
        {
            if (dto.Items == null || !dto.Items.Any())
                throw new Exception("Vui lòng chọn ít nhất 1 sản phẩm.");

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.Status == 1);
            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng.");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == employeeUserId && e.Status == 1);

            var validPaymentMethods = new[] { "Chuyển khoản", "Momo", "Tiền mặt" };
            if (!validPaymentMethods.Contains(dto.PaymentMethod))
                throw new Exception("Phương thức thanh toán không hợp lệ.");

            var shipper = await _context.Shippers
                .FirstOrDefaultAsync(s => s.Status == 1);
            if (shipper == null)
                throw new Exception("Không có đơn vị vận chuyển hoạt động.");

            var mergedItems = dto.Items
                .GroupBy(i => i.ProductVariantId)
                .Select(g => new { VariantId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            if (mergedItems.Any(x => x.Quantity <= 0))
                throw new Exception("Số lượng sản phẩm phải lớn hơn 0.");

            var variantIds = mergedItems.Select(x => x.VariantId).ToList();
            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.Id) && v.Status == 1)
                .ToDictionaryAsync(v => v.Id);

            foreach (var req in mergedItems)
            {
                if (!variants.TryGetValue(req.VariantId, out var variant) ||
                    variant.Product == null ||
                    variant.Product.Status != 1 ||
                    variant.Price == null ||
                    variant.Price <= 0)
                {
                    throw new Exception($"Biến thể sản phẩm ID {req.VariantId} không hợp lệ hoặc ngừng kinh doanh.");
                }

                var availableStock = await _context.Inventories
                    .Where(i => i.VariantId == variant.Id && i.Status == 1)
                    .SumAsync(i => i.Quantity ?? 0);

                if (req.Quantity > availableStock)
                    throw new Exception($"Sản phẩm {variant.Name} không đủ tồn kho. Hiện còn {availableStock}.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    CustomerId = customer.Id,
                    EmployeeId = employee?.Id,
                    OrderDate = DateTime.Now,
                    Freight = 0,
                    Receive = string.IsNullOrWhiteSpace(dto.Receive) ? "Mua tại quầy" : dto.Receive.Trim(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = employeeUserId,
                    Notes = dto.Notes,
                    Status = 0
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;

                foreach (var req in mergedItems)
                {
                    var variant = variants[req.VariantId];
                    var product = variant.Product!;
                    decimal unitPrice = variant.Price!.Value;
                    decimal lineTotal = unitPrice * req.Quantity;
                    totalAmount += lineTotal;

                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        ProductVariantId = variant.Id,
                        UnitPrice = unitPrice,
                        Quantity = (short)req.Quantity,
                        ProductName = product.ProductName,
                        Sku = variant.Sku,
                        VariantInfo = variant.Name,
                        LineTotal = lineTotal,
                        Discount = 0,
                        CreatedAt = DateTime.Now,
                        CreatedBy = employeeUserId,
                        Status = 1
                    });

                    var inventories = await _context.Inventories
                        .Where(i => i.VariantId == variant.Id && i.Status == 1 && (i.Quantity ?? 0) > 0)
                        .OrderBy(i => i.Id)
                        .ToListAsync();

                    int remaining = req.Quantity;
                    foreach (var inv in inventories)
                    {
                        if (remaining <= 0) break;
                        int currentQty = inv.Quantity ?? 0;
                        int deduct = Math.Min(currentQty, remaining);
                        inv.Quantity = currentQty - deduct;
                        inv.UpdatedAt = DateTime.Now;
                        inv.UpdatedBy = employeeUserId;
                        remaining -= deduct;
                    }

                    if (remaining > 0)
                        throw new Exception($"Không thể trừ đủ tồn kho cho {variant.Name}.");

                    variant.StockQuantity = inventories.Where(i => i.Status == 1).Sum(i => i.Quantity ?? 0);
                    variant.UpdatedAt = DateTime.Now;
                    variant.UpdatedBy = employeeUserId;
                }

                _context.Payments.Add(new Payment
                {
                    OrderId = order.Id,
                    Amount = totalAmount,
                    Method = dto.PaymentMethod,
                    CreatedAt = DateTime.Now,
                    CreatedBy = employeeUserId,
                    Notes = "Nhân viên tạo đơn dùm khách",
                    Status = 0
                });

                _context.ShipInfos.Add(new ShipInfo
                {
                    OrderId = order.Id,
                    ShipperId = shipper.Id,
                    ShipName = dto.ShipName,
                    ShipPhone = dto.ShipPhone,
                    ShipAddress = dto.ShipAddress,
                    ShipCity = dto.ShipCity,
                    ShipRegion = dto.ShipRegion,
                    ShipCountry = dto.ShipCountry ?? "Vietnam",
                    CreatedAt = DateTime.Now,
                    CreatedBy = employeeUserId,
                    Notes = order.Receive,
                    Status = 1
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var created = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Employee)
                    .Include(o => o.CreatedByNavigation)
                    .Include(o => o.OrderDetails)
                    .Include(o => o.Payments)
                    .Include(o => o.ShipInfos)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                var response = MapSingleOrder(created!);
                if (dto.PaymentMethod == "Chuyển khoản")
                {
                    response.TransferContent = $"THANHTOAN {created!.Id}";
                    response.BankName = _configuration["BankTransfer:BankName"];
                    response.BankAccount = _configuration["BankTransfer:AccountNumber"];
                }

                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private OrderResponseDTO MapSingleOrder(Order order)
        {
            var dto = _mapper.Map<OrderResponseDTO>(order);
            dto.TotalAmount = order.OrderDetails.Sum(d =>
                d.LineTotal ?? (d.UnitPrice * d.Quantity))
                + (order.Freight ?? 0);

            if (order.Customer != null)
            {
                dto.CustomerName = !string.IsNullOrWhiteSpace(order.Customer.ContactName)
                    ? order.Customer.ContactName
                    : order.Customer.Email;
            }

            if (order.Employee != null)
            {
                dto.EmployeeName = $"{order.Employee.FirstName} {order.Employee.LastName}".Trim();
            }
            else if (order.CreatedBy != null &&
                     order.Customer != null &&
                     order.Customer.UserId != order.CreatedBy)
            {
                dto.EmployeeName = order.CreatedByNavigation?.FullName;
            }

            return dto;
        }

        public async Task<int> GetOrderBadgeCountAsync(int userId, string role)
        {
            if (role == "Admin")
            {
                // Admin: đếm đơn đang chờ duyệt (Status == 0)
                return await _context.Orders.CountAsync(o => o.Status == 0);
            }

            if (role == "Employee")
            {
                // Employee: đếm đơn do chính nhân viên đó đặt dùm còn đang xử lý / đang giao
                var employee = await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.Status == 1);

                return await _context.Orders.CountAsync(o =>
                    (o.Status == 0 || o.Status == 1) &&
                    ((employee != null && o.EmployeeId == employee.Id) ||
                     (o.CreatedBy == userId && (o.Customer == null || o.Customer.UserId != userId))));
            }

            // Customer: đếm đơn của khách hàng còn đang xử lý / đang giao
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == 1);

            if (customer == null) return 0;

            return await _context.Orders.CountAsync(o =>
                o.CustomerId == customer.Id &&
                (o.Status == 0 || o.Status == 1));
        }


        // ============================================================
        // 6. Cập nhật trạng thái đơn hàng (Admin, Employee)
        // ============================================================
        public async Task<bool> UpdateOrderStatusAsync(
            int orderId,
            int status,
            int userId)
        {
            // Các trạng thái hợp lệ: 0, 1, 2, 3
            if (status < 0 || status > 3)
                return false;

            // Tìm đơn hàng
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            // Không cho thay đổi đơn đã hoàn thành hoặc đã hủy
            if (order.Status == 2 || order.Status == 3)
                return false;

            // Không cho chuyển trạng thái ngược
            if (status < order.Status)
                return false;

            // Nếu chuyển sang trạng thái hủy thì hoàn lại kho
            using var transaction = await _context.Database
                .BeginTransactionAsync();

            try
            {
                if (status == 3 && order.Status != 3)
                {
                    await RestoreInventoryAsync(order, userId);
                }

                // Cập nhật trạng thái
                order.Status = status;
                order.UpdatedAt = DateTime.Now;
                order.UpdatedBy = userId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // ============================================================
        // 7. Khách hàng hủy đơn hàng của mình
        // ============================================================
        public async Task<bool> CancelMyOrderAsync(
            int orderId,
            int userId)
        {
            // 1. Tìm khách hàng từ UserId
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.Status == 1);

            if (customer == null)
                return false;

            // 2. Tìm đơn hàng thuộc khách hàng
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.CustomerId == customer.Id);

            if (order == null)
                return false;

            // 3. Chỉ cho phép hủy đơn đang chờ xử lý
            if (order.Status != 0)
                return false;

            using var transaction = await _context.Database
                .BeginTransactionAsync();

            try
            {
                // 4. Hoàn lại tồn kho
                await RestoreInventoryAsync(order, userId);

                // 5. Cập nhật trạng thái đơn hàng thành đã hủy
                order.Status = 3;
                order.UpdatedAt = DateTime.Now;
                order.UpdatedBy = userId;

                // 6. Lưu và commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // ============================================================
        // 8. Hàm hỗ trợ hoàn lại tồn kho khi hủy đơn
        // ============================================================
        private async Task RestoreInventoryAsync(Order order, int userId)
        {
            foreach (var detail in order.OrderDetails)
            {
                // Chi tiết đơn không có ProductVariant thì bỏ qua
                if (detail.ProductVariantId == null)
                    continue;

                // Tìm biến thể sản phẩm
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v =>
                        v.Id == detail.ProductVariantId.Value);

                if (variant == null)
                    continue;

                // Lấy danh sách kho đang hoạt động của biến thể
                var inventories = await _context.Inventories
                    .Where(i =>
                        i.VariantId == variant.Id &&
                        i.Status == 1)
                    .OrderBy(i => i.Id)
                    .ToListAsync();

                // Nếu đã có kho hoạt động thì cộng lại số lượng
                if (inventories.Any())
                {
                    inventories[0].Quantity =
                        (inventories[0].Quantity ?? 0) + detail.Quantity;

                    inventories[0].UpdatedAt = DateTime.Now;
                    inventories[0].UpdatedBy = userId;
                }
                else
                {
                    // Nếu không có kho hoạt động thì tạo kho mới
                    var inventory = new Inventory
                    {
                        VariantId = variant.Id,
                        Quantity = detail.Quantity,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                        Notes = $"Hoàn kho từ đơn hàng #{order.Id}",
                        Status = 1
                    };

                    _context.Inventories.Add(inventory);

                    // Thêm kho mới vào danh sách để đồng bộ số lượng
                    inventories.Add(inventory);
                }

                // Đồng bộ tồn kho của ProductVariant
                variant.StockQuantity = inventories.Sum(i =>
                    i.Quantity ?? 0);

                variant.UpdatedAt = DateTime.Now;
                variant.UpdatedBy = userId;
            }
        }
    }
}