using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    public class OptionPresetController : ControllerBase
    {
        private readonly IOptionPresetService _service;

        public OptionPresetController(IOptionPresetService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            return Ok(await _service.GetActiveAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var preset = await _service.GetByIdAsync(id);
            if (preset == null) return NotFound(new { message = "Không tìm thấy Option mẫu." });
            return Ok(preset);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OptionPresetUpsertDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (success, message, preset) = await _service.CreateAsync(dto);
            if (!success) return BadRequest(new { message });
            return Ok(new { message, data = preset });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, OptionPresetUpsertDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (success, message) = await _service.UpdateAsync(id, dto);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _service.DeleteAsync(id);
            if (!success) return NotFound(new { message });
            return Ok(new { message });
        }
    }
}
