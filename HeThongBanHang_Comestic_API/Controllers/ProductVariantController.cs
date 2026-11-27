
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _productVariantService;

        public ProductVariantController(
            IProductVariantService productVariantService)
        {
            _productVariantService = productVariantService;
        }

        // GET: api/ProductVariant/product/5
        // Lấy danh sách biến thể theo sản phẩm
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var result = await _productVariantService.GetByProductAsync(productId);

            return Ok(result);
        }

        // GET: api/ProductVariant/product/5/admin
        // Admin, Employee: lấy toàn bộ biến thể kể cả đã ẩn
        [HttpGet("product/{productId}/admin")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetByProductForAdmin(int productId)
        {
            var result = await _productVariantService.GetByProductForAdminAsync(productId);

            return Ok(result);
        }

        // GET: api/ProductVariant/5
        // Lấy chi tiết biến thể
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productVariantService.GetByIdAsync(id);

            if (result == null)
                return NotFound("Không tìm thấy biến thể sản phẩm.");

            return Ok(result);
        }

        // POST: api/ProductVariant
        // Thêm biến thể mới
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(ProductVariantCreateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _productVariantService.CreateAsync(dto, userId);

            if (!result)
                return BadRequest(new { message = "Không thể thêm biến thể. Vui lòng kiểm tra ProductId, SKU và các giá trị nhập vào." });

            return Ok(new { message = "Thêm biến thể sản phẩm thành công." });
        }

        // PUT: api/ProductVariant/5
        // Cập nhật biến thể
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, ProductVariantUpdateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _productVariantService.UpdateAsync(id, dto, userId);

            if (!result)
                return BadRequest(new { message = "Không thể cập nhật biến thể. Vui lòng kiểm tra ID, SKU và các giá trị nhập vào." });

            return Ok(new { message = "Cập nhật biến thể sản phẩm thành công." });
        }

        // PATCH: api/ProductVariant/5/status/0
        // Ẩn hoặc kích hoạt lại biến thể
        [HttpPatch("{id}/status/{status}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _productVariantService.UpdateStatusAsync(id, status, userId);

            if (!result)
                return BadRequest(new { message = "Không thể cập nhật trạng thái biến thể." });

            return Ok(new { message = "Cập nhật trạng thái biến thể thành công." });
        }

        // PUT: api/ProductVariant/product/5/options
        // Đồng bộ nhóm thuộc tính (Dung tích, Mùi hương...) cho sản phẩm
        [HttpPut("product/{productId}/options")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> SyncOptions(int productId, ProductOptionsSyncDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _productVariantService.SyncOptionsAsync(productId, dto, userId);
            if (!result)
                return BadRequest(new { message = "Không thể đồng bộ thuộc tính sản phẩm." });

            return Ok(new { message = "Đồng bộ thuộc tính sản phẩm thành công." });
        }
    }
}
