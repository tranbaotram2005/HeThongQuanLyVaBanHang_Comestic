
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductOptionValueController : ControllerBase
    {
        private readonly IProductOptionValueService _service;

        public ProductOptionValueController(IProductOptionValueService service)
        {
            _service = service;
        }

        // =====================================
        // GET: api/ProductOptionValue/option/5
        // Lấy danh sách giá trị theo thuộc tính
        // =====================================
        [HttpGet("option/{productOptionId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByOption(int productOptionId)
        {
            var values = await _service.GetByOptionAsync(productOptionId);

            return Ok(values);
        }

        // =====================================
        // GET: api/ProductOptionValue/5
        // Lấy chi tiết giá trị theo ID
        // =====================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var value = await _service.GetByIdAsync(id);

            if (value == null)
                return NotFound(new
                {
                    message = "Không tìm thấy giá trị thuộc tính."
                });

            return Ok(value);
        }

        // =====================================
        // POST: api/ProductOptionValue
        // Thêm giá trị thuộc tính
        // =====================================
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(ProductOptionValueCreateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _service.CreateAsync(dto, userId);

            if (!result)
                return BadRequest(new
                {
                    message = "Không thể thêm giá trị. Vui lòng kiểm tra thuộc tính và dữ liệu đầu vào."
                });

            return Ok(new
            {
                message = "Thêm giá trị thuộc tính thành công."
            });
        }

        // =====================================
        // PUT: api/ProductOptionValue/5
        // Cập nhật giá trị thuộc tính
        // =====================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, ProductOptionValueUpdateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _service.UpdateAsync(id, dto, userId);

            if (!result)
                return BadRequest(new
                {
                    message = "Không thể cập nhật giá trị. Vui lòng kiểm tra ID và dữ liệu đầu vào."
                });

            return Ok(new
            {
                message = "Cập nhật giá trị thuộc tính thành công."
            });
        }

        // =====================================
        // PATCH: api/ProductOptionValue/5/status
        // Ẩn hoặc hiện giá trị
        // =====================================
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] int status)
        {
            if (status != 0 && status != 1)
                return BadRequest(new
                {
                    message = "Status chỉ được nhận giá trị 0 hoặc 1."
                });

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _service.UpdateStatusAsync(id, status, userId);

            if (!result)
                return NotFound(new
                {
                    message = "Không tìm thấy giá trị thuộc tính."
                });

            return Ok(new
            {
                message = status == 1
                    ? "Hiện giá trị thuộc tính thành công."
                    : "Ẩn giá trị thuộc tính thành công."
            });
        }
    }
}
