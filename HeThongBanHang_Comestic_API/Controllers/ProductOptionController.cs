
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductOptionController : ControllerBase
    {
        private readonly IProductOptionService _productOptionService;

        public ProductOptionController(IProductOptionService productOptionService)
        {
            _productOptionService = productOptionService;
        }

        // GET: api/ProductOption/product/5
        // Lấy danh sách thuộc tính theo sản phẩm
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var options = await _productOptionService.GetByProductAsync(productId);

            return Ok(options);
        }

        // GET: api/ProductOption/5
        // Lấy chi tiết thuộc tính
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var option = await _productOptionService.GetByIdAsync(id);

            if (option == null)
                return NotFound(new
                {
                    message = "Không tìm thấy thuộc tính sản phẩm."
                });

            return Ok(option);
        }

        // POST: api/ProductOption
        // Thêm thuộc tính
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(ProductOptionCreateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _productOptionService.CreateAsync(dto, userId);

            if (!result)
                return BadRequest(new
                {
                    message = "Không thể thêm thuộc tính. Vui lòng kiểm tra sản phẩm và dữ liệu đầu vào."
                });

            return Ok(new
            {
                message = "Thêm thuộc tính sản phẩm thành công."
            });
        }

        // PUT: api/ProductOption/5
        // Cập nhật thuộc tính
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, ProductOptionUpdateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _productOptionService.UpdateAsync(id, dto, userId);

            if (!result)
                return BadRequest(new
                {
                    message = "Không thể cập nhật thuộc tính. Vui lòng kiểm tra ID và dữ liệu đầu vào."
                });

            return Ok(new
            {
                message = "Cập nhật thuộc tính thành công."
            });
        }

        // PATCH: api/ProductOption/5/status?status=0
        // Ẩn hoặc hiện thuộc tính
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

            var result = await _productOptionService.UpdateStatusAsync(id, status, userId);

            if (!result)
                return NotFound(new
                {
                    message = "Không tìm thấy thuộc tính sản phẩm."
                });

            return Ok(new
            {
                message = status == 1
                    ? "Hiện thuộc tính thành công."
                    : "Ẩn thuộc tính thành công."
            });
        }
    }
}
