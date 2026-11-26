
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
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        // Lấy danh sách sản phẩm yêu thích
        [HttpGet]
        public async Task<IActionResult> GetMyWishlist()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _wishlistService.GetMyWishlistAsync(userId);

            return Ok(result);
        }

        // Thêm sản phẩm vào danh sách yêu thích
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] WishlistCreateDTO dto)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _wishlistService.CreateAsync(dto, userId);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Không thể thêm sản phẩm yêu thích. Sản phẩm không tồn tại, không hoạt động hoặc đã có trong danh sách."
                });
            }

            return Ok(new
            {
                message = "Thêm sản phẩm vào danh sách yêu thích thành công."
            });
        }

        // Xóa sản phẩm khỏi danh sách yêu thích
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _wishlistService.DeleteAsync(id, userId);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy sản phẩm yêu thích."
                });
            }

            return Ok(new
            {
                message = "Xóa sản phẩm khỏi danh sách yêu thích thành công."
            });
        }
    }
}
