using System.Security.Claims;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // =====================================
        // GET: api/Product
        // Lấy danh sách sản phẩm đang hoạt động
        // Khách chưa đăng nhập cũng được xem
        // =====================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();

            return Ok(products);
        }

        // =====================================
        // GET: api/Product/admin
        // Admin & Employee: lấy toàn bộ sản phẩm (kể cả đã ẩn)
        // =====================================
        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var products = await _productService.GetAllForAdminAsync();

            return Ok(products);
        }

        // =====================================
        // GET: api/Product/5
        // Lấy chi tiết sản phẩm theo ID
        // =====================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound(new
                {
                    message = "Không tìm thấy sản phẩm."
                });

            return Ok(product);
        }

        // =====================================
        // GET: api/Product/category/2
        // Lấy sản phẩm theo danh mục
        // =====================================
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _productService.GetByCategoryAsync(categoryId);

            return Ok(products);
        }

        // =====================================
        // POST: api/Product
        // Thêm sản phẩm
        // Chỉ Admin và Employee
        // =====================================
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(ProductCreateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var newProductId = await _productService.CreateAndReturnIdAsync(dto, userId);

            if (!newProductId.HasValue)
                return BadRequest(new
                {
                    message = "Không thể thêm sản phẩm. Vui lòng kiểm tra dữ liệu đầu vào, danh mục và nhà cung cấp."
                });

            return Ok(new
            {
                message = "Thêm sản phẩm thành công.",
                id = newProductId.Value
            });
        }

        // =====================================
        // PUT: api/Product/5
        // Cập nhật sản phẩm
        // Chỉ Admin và Employee
        // =====================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, ProductUpdateDTO dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
                return Unauthorized();

            var result = await _productService.UpdateAsync(id, dto, userId);

            if (!result)
                return BadRequest(new
                {
                    message = "Không thể cập nhật sản phẩm. Vui lòng kiểm tra ID và dữ liệu đầu vào."
                });

            return Ok(new
            {
                message = "Cập nhật sản phẩm thành công."
            });
        }

        // =====================================
        // PATCH: api/Product/5/status
        // Ẩn hoặc hiện sản phẩm
        // status = 0: Ẩn
        // status = 1: Hiện
        // Chỉ Admin và Employee
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

            var result = await _productService.UpdateStatusAsync(id, status, userId);

            if (!result)
                return NotFound(new
                {
                    message = "Không tìm thấy sản phẩm."
                });

            return Ok(new
            {
                message = status == 1
                    ? "Hiện sản phẩm thành công."
                    : "Ẩn sản phẩm thành công."
            });
        }

        // =====================================
        // GET: api/Product/filter-price
        // Lọc sản phẩm theo khoảng giá
        // =====================================
        [HttpGet("filter-price")]
        [AllowAnonymous]
        public async Task<IActionResult> FilterByPrice(
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            // Kiểm tra giá không được âm
            if (minPrice < 0 || maxPrice < 0)
            {
                return BadRequest(new
                {
                    message = "Giá lọc không được âm."
                });
            }

            // Kiểm tra giá thấp nhất không lớn hơn giá cao nhất
            if (minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice.Value > maxPrice.Value)
            {
                return BadRequest(new
                {
                    message = "Giá thấp nhất không được lớn hơn giá cao nhất."
                });
            }

            var products = await _productService.FilterByPriceAsync(
                minPrice,
                maxPrice);

            return Ok(products);
        }
    }
}
