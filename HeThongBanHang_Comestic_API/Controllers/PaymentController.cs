using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.DTOs.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPaymentService paymentService,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        // Lấy UserId từ JWT
        private int GetUserId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) ||
                !int.TryParse(userId, out int id))
            {
                throw new UnauthorizedAccessException(
                    "Không xác định được người dùng.");
            }

            return id;
        }

        // =====================================================
        // 1. CUSTOMER - XEM THANH TOÁN CỦA ĐƠN MÌNH
        // GET: api/Payment/my/order/5
        // =====================================================
        [HttpGet("my/order/{orderId:int}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyPayments(int orderId)
        {
            int userId = GetUserId();

            var result = await _paymentService.GetMyPaymentsAsync(
                userId, orderId);

            return Ok(result);
        }

        // =====================================================
        // 2. ADMIN, EMPLOYEE - XEM THANH TOÁN THEO ĐƠN
        // GET: api/Payment/order/5
        // =====================================================
        [HttpGet("order/{orderId:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetPaymentsByOrderId(
            int orderId)
        {
            var result = await _paymentService
                .GetPaymentsByOrderIdAsync(orderId);

            return Ok(result);
        }

        // =====================================================
        // 3. ADMIN, EMPLOYEE - XEM TẤT CẢ THANH TOÁN
        // GET: api/Payment
        // =====================================================
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetAllPayments()
        {
            var result = await _paymentService.GetAllPaymentsAsync();

            return Ok(result);
        }

        // =====================================================
        // 4. ADMIN, EMPLOYEE - XEM CHI TIẾT THANH TOÁN
        // GET: api/Payment/5
        // =====================================================
        [HttpGet("{paymentId:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetPaymentById(
            int paymentId)
        {
            var result = await _paymentService
                .GetPaymentByIdAsync(paymentId);

            if (result == null)
                return NotFound("Không tìm thấy thanh toán.");

            return Ok(result);
        }

        // =====================================================
        // 5. ADMIN, EMPLOYEE - TẠO BẢN GHI THANH TOÁN
        // POST: api/Payment
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CreatePayment(
            PaymentCreateDTO dto)
        {
            try
            {
                int userId = GetUserId();

                var result = await _paymentService
                    .CreatePaymentAsync(dto, userId);

                if (result == null)
                    return BadRequest("Không thể tạo thanh toán.");

                return Ok(new
                {
                    message = "Tạo thanh toán thành công.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // 6. ADMIN, EMPLOYEE - CẬP NHẬT TRẠNG THÁI THANH TOÁN
        // PUT: api/Payment/5/status
        // =====================================================
        [HttpPut("{paymentId:int}/status")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdatePaymentStatus(
            int paymentId,
            PaymentUpdateStatusDTO dto)
        {
            try
            {
                int userId = GetUserId();

                bool result = await _paymentService
                    .UpdatePaymentStatusAsync(
                        paymentId, dto, userId);

                if (!result)
                    return NotFound("Không tìm thấy thanh toán.");

                return Ok(new
                {
                    message =
                        "Cập nhật trạng thái thanh toán thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // SEPAY WEBHOOK
        // =====================================================
        [HttpPost("sepay-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> SePayWebhook(
            [FromBody] SePayWebhookDTO dto)
        {
            // 1. Lấy API Key từ cấu hình
            string? expectedApiKey = _configuration["SePay:ApiKey"];

            if (string.IsNullOrWhiteSpace(expectedApiKey))
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Server chưa cấu hình SePay API Key."
                });
            }

            // 2. Lấy API Key từ request
            string authorization =
                Request.Headers["Authorization"].ToString();

            if (!authorization.StartsWith(
                "Apikey ",
                StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Authorization không hợp lệ."
                });
            }

            string receivedApiKey =
                authorization.Substring(7).Trim();

            // 3. Kiểm tra API Key
            byte[] expectedBytes =
                Encoding.UTF8.GetBytes(expectedApiKey);

            byte[] receivedBytes =
                Encoding.UTF8.GetBytes(receivedApiKey);

            bool isValid =
                expectedBytes.Length == receivedBytes.Length &&
                CryptographicOperations.FixedTimeEquals(
                    expectedBytes,
                    receivedBytes);

            if (!isValid)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "API Key không hợp lệ."
                });
            }

            // 4. Gọi PaymentService xử lý giao dịch
            bool result =
                await _paymentService.ProcessSePayWebhookAsync(dto);

            // 5. Trả kết quả
            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = "Thanh toán thành công, đơn hàng đã được cập nhật.",
                    transactionId = dto.Id,
                    transferAmount = dto.TransferAmount
                });
            }

            // Webhook nhận được nhưng không khớp đơn hàng
            // Không trả lỗi HTTP để tránh SePay gửi lặp liên tục
            return Ok(new
            {
                success = true,
                message = "Đã nhận giao dịch nhưng không tìm thấy đơn hàng phù hợp hoặc giao dịch không hợp lệ.",
                transactionId = dto.Id
            });
        }
    }
}