
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
    public class CartItemController : ControllerBase
    {
        private readonly ICartItemService _cartItemService;

        public CartItemController(ICartItemService cartItemService)
        {
            _cartItemService = cartItemService;
        }

        // Lấy danh sách sản phẩm trong giỏ hàng của người đăng nhập
        [HttpGet]
        public async Task<IActionResult> GetMyCartItems()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _cartItemService.GetMyCartItemsAsync(userId);

            return Ok(result);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CartItemCreateDTO dto)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _cartItemService.CreateAsync(dto, userId);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Không thể thêm sản phẩm vào giỏ hàng. Vui lòng kiểm tra sản phẩm và số lượng."
                });
            }

            return Ok(new
            {
                message = "Thêm sản phẩm vào giỏ hàng thành công."
            });
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CartItemUpdateDTO dto)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _cartItemService.UpdateAsync(id, dto, userId);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Không thể cập nhật sản phẩm trong giỏ hàng."
                });
            }

            return Ok(new
            {
                message = "Cập nhật số lượng thành công."
            });
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _cartItemService.DeleteAsync(id, userId);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy sản phẩm trong giỏ hàng."
                });
            }

            return Ok(new
            {
                message = "Xóa sản phẩm khỏi giỏ hàng thành công."
            });
        }
    }
}
