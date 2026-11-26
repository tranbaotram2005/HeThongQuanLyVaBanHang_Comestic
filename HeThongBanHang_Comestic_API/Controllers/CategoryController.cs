
using System.Security.Claims;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // =========================================
        // 1. LẤY DANH SÁCH DANH MỤC
        // CUSTOMER, STAFF, ADMIN
        // =========================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();

            return Ok(categories);
        }

        // =========================================
        // 2. THÊM DANH MỤC
        // STAFF, ADMIN
        // =========================================
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(
            [FromBody] CategoryCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest("Tên danh mục không được để trống.");

            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _categoryService.CreateAsync(dto, userId);

            return Ok("Thêm danh mục thành công.");
        }

        // =========================================
        // 3. CẬP NHẬT DANH MỤC
        // STAFF, ADMIN
        // =========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CategoryUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest("Tên danh mục không được để trống.");

            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _categoryService.UpdateAsync(
                id, dto, userId
            );

            if (!result)
                return NotFound("Không tìm thấy danh mục.");

            return Ok("Cập nhật danh mục thành công.");
        }

        // =========================================
        // 4. ẨN / HIỆN DANH MỤC
        // STAFF, ADMIN
        // =========================================
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] CategoryStatusDTO dto)
        {
            if (dto.Status != 0 && dto.Status != 1)
                return BadRequest("Status chỉ nhận giá trị 0 hoặc 1.");

            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _categoryService.UpdateStatusAsync(
                id, dto.Status, userId
            );

            if (!result)
                return NotFound("Không tìm thấy danh mục.");

            return Ok(dto.Status == 1
                ? "Hiển thị danh mục thành công."
                : "Ẩn danh mục thành công.");
        }
    }
}
