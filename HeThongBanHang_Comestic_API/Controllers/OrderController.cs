
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Lấy UserId từ JWT
        private int GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) ||
                !int.TryParse(userId, out int id))
            {
                throw new UnauthorizedAccessException(
                    "Không xác định được người dùng.");
            }

            return id;
        }

        // =====================================================
        // 1. CUSTOMER - TẠO ĐƠN HÀNG
        // POST: api/Order
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder(OrderCreateDTO dto)
        {
            try
            {
                int userId = GetUserId();

                var result = await _orderService.CreateOrderAsync(
                    dto, userId);

                if (result == null)
                    return BadRequest("Không thể tạo đơn hàng.");

                return Ok(new
                {
                    message = "Tạo đơn hàng thành công.",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =====================================================
        // 2. CUSTOMER - LẤY DANH SÁCH ĐƠN CỦA MÌNH
        // GET: api/Order/my
        // =====================================================
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                int userId = GetUserId();

                var result = await _orderService.GetMyOrdersAsync(userId);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // =====================================================
        // 3. CUSTOMER - LẤY CHI TIẾT ĐƠN CỦA MÌNH
        // GET: api/Order/my/5
        // =====================================================
        [HttpGet("my/{orderId:int}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrderById(int orderId)
        {
            try
            {
                int userId = GetUserId();

                var result = await _orderService.GetMyOrderByIdAsync(
                    orderId, userId);

                if (result == null)
                    return NotFound("Không tìm thấy đơn hàng.");

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // =====================================================
        // 4. CUSTOMER - HỦY ĐƠN CỦA MÌNH
        // PUT: api/Order/my/5/cancel
        // =====================================================
        [HttpPut("my/{orderId:int}/cancel")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelMyOrder(int orderId)
        {
            try
            {
                int userId = GetUserId();

                bool result = await _orderService.CancelMyOrderAsync(
                    orderId, userId);

                if (!result)
                    return NotFound("Không tìm thấy đơn hàng.");

                return Ok(new
                {
                    message = "Hủy đơn hàng thành công."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =====================================================
        // 5. ADMIN, EMPLOYEE - LẤY TẤT CẢ ĐƠN HÀNG
        // GET: api/Order
        // =====================================================
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _orderService.GetAllOrdersAsync();

            return Ok(result);
        }

        // =====================================================
        // 6. ADMIN, EMPLOYEE - LẤY CHI TIẾT ĐƠN HÀNG
        // GET: api/Order/5
        // =====================================================
        [HttpGet("{orderId:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var result = await _orderService.GetOrderByIdAsync(orderId);

            if (result == null)
                return NotFound("Không tìm thấy đơn hàng.");

            return Ok(result);
        }

        // =====================================================
        // 7. ADMIN, EMPLOYEE - CẬP NHẬT TRẠNG THÁI ĐƠN
        // PUT: api/Order/5/status?status=1
        // =====================================================
        [HttpPut("{orderId:int}/status")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateOrderStatus(
            int orderId,
            [FromQuery] int status)
        {
            try
            {
                int userId = GetUserId();

                bool result = await _orderService.UpdateOrderStatusAsync(
                    orderId, status, userId);

                if (!result)
                    return NotFound("Không tìm thấy đơn hàng.");

                return Ok(new
                {
                    message = "Cập nhật trạng thái đơn hàng thành công."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
