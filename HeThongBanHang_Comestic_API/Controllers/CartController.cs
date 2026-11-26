
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // Lấy giỏ hàng của người dùng đang đăng nhập
        // GET: api/Cart
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _cartService.GetMyCartAsync(userId);

            if (result == null)
                return NotFound("Bạn chưa có giỏ hàng.");

            return Ok(result);
        }

        // Tạo giỏ hàng mới
        // POST: api/Cart
        [HttpPost]
        public async Task<IActionResult> Create(CartCreateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _cartService.CreateAsync(dto, userId);

            if (!result)
                return BadRequest("Bạn đã có giỏ hàng hoặc không thể tạo giỏ hàng.");

            return Ok("Tạo giỏ hàng thành công.");
        }

        // Cập nhật mã giảm giá và ghi chú
        // PUT: api/Cart
        [HttpPut]
        public async Task<IActionResult> Update(CartUpdateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _cartService.UpdateAsync(dto, userId);

            if (!result)
                return NotFound("Không tìm thấy giỏ hàng.");

            return Ok("Cập nhật giỏ hàng thành công.");
        }

        // Xóa giỏ hàng
        // DELETE: api/Cart
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _cartService.DeleteAsync(userId);

            if (!result)
                return NotFound("Không tìm thấy giỏ hàng.");

            return Ok("Xóa giỏ hàng thành công.");
        }
    }
}
