using System.Security.Claims;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _supplierService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _supplierService.GetByIdAsync(id);
            if (result == null) return NotFound("Không tìm thấy nhà cung cấp.");
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create([FromBody] SupplierCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                return BadRequest("Tên công ty không được để trống.");

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized();

            await _supplierService.CreateAsync(dto, userId);
            return Ok("Thêm nhà cung cấp thành công.");
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Update(int id, [FromBody] SupplierUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                return BadRequest("Tên công ty không được để trống.");

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized();

            var result = await _supplierService.UpdateAsync(id, dto, userId);
            if (!result) return NotFound("Không tìm thấy nhà cung cấp.");
            return Ok("Cập nhật nhà cung cấp thành công.");
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] SupplierStatusDTO dto)
        {
            if (dto.Status != 0 && dto.Status != 1)
                return BadRequest("Status chỉ nhận giá trị 0 hoặc 1.");

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized();

            var result = await _supplierService.UpdateStatusAsync(id, dto.Status, userId);
            if (!result) return NotFound("Không tìm thấy nhà cung cấp.");

            return Ok(dto.Status == 1 ? "Hiển thị nhà cung cấp thành công." : "Ẩn nhà cung cấp thành công.");
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var result = await _supplierService.GetAllForAdminAsync();
            return Ok(result);
        }
    }
}