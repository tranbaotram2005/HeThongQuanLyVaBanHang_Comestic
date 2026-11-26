
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVariantOptionValueController : ControllerBase
    {
        private readonly IProductVariantOptionValueService _service;

        public ProductVariantOptionValueController(
            IProductVariantOptionValueService service)
        {
            _service = service;
        }

        // GET: api/ProductVariantOptionValue/variant/5
        // Lấy danh sách giá trị thuộc tính theo biến thể
        [HttpGet("variant/{productVariantId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByVariant(int productVariantId)
        {
            var result = await _service.GetByVariantAsync(productVariantId);

            return Ok(result);
        }

        // GET: api/ProductVariantOptionValue/5
        // Lấy chi tiết một bản ghi liên kết
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound("Không tìm thấy giá trị thuộc tính của biến thể.");

            return Ok(result);
        }

        // POST: api/ProductVariantOptionValue
        // Gán giá trị thuộc tính cho biến thể
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(ProductVariantOptionValueCreateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _service.CreateAsync(dto, userId);

            if (!result)
                return BadRequest(
                    "Không thể gán giá trị thuộc tính. Vui lòng kiểm tra biến thể, giá trị thuộc tính, sản phẩm và dữ liệu trùng.");

            return Ok("Gán giá trị thuộc tính cho biến thể thành công.");
        }

        // PUT: api/ProductVariantOptionValue/5
        // Cập nhật giá trị thuộc tính của biến thể
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(
            int id,
            ProductVariantOptionValueUpdateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _service.UpdateAsync(id, dto, userId);

            if (!result)
                return BadRequest(
                    "Không thể cập nhật. Vui lòng kiểm tra ID, giá trị thuộc tính và dữ liệu trùng.");

            return Ok("Cập nhật giá trị thuộc tính thành công.");
        }

        // PATCH: api/ProductVariantOptionValue/5/status/0
        // Ẩn hoặc kích hoạt lại bản ghi liên kết
        [HttpPatch("{id}/status/{status}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _service.UpdateStatusAsync(id, status, userId);

            if (!result)
                return BadRequest("Không thể cập nhật trạng thái bản ghi.");

            return Ok("Cập nhật trạng thái thành công.");
        }
    }
}
