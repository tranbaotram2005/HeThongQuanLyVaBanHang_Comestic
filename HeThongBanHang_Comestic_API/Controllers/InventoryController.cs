
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // Lấy danh sách tồn kho
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _inventoryService.GetAllAsync();

            return Ok(result);
        }

        // Lấy tồn kho theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _inventoryService.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy thông tin tồn kho."
                });
            }

            return Ok(result);
        }

        // Lấy tồn kho theo biến thể sản phẩm
        [HttpGet("variant/{variantId}")]
        public async Task<IActionResult> GetByVariant(int variantId)
        {
            var result = await _inventoryService.GetByVariantAsync(variantId);

            return Ok(result);
        }

        // Tạo bản ghi tồn kho
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] InventoryCreateDTO dto)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _inventoryService.CreateAsync(dto, userId);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Không thể tạo bản ghi tồn kho. Biến thể không hợp lệ, số lượng không hợp lệ hoặc bản ghi đã tồn tại."
                });
            }

            return Ok(new
            {
                message = "Tạo bản ghi tồn kho thành công."
            });
        }

        // Cập nhật thông tin tồn kho
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] InventoryUpdateDTO dto)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _inventoryService.UpdateAsync(id, dto, userId);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Không thể cập nhật tồn kho. Bản ghi không tồn tại, dữ liệu không hợp lệ, bị trùng hoặc đã được người khác cập nhật."
                });
            }

            return Ok(new
            {
                message = "Cập nhật tồn kho thành công."
            });
        }

        // Cập nhật trạng thái tồn kho
        [HttpPatch("{id}/status/{status}")]
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _inventoryService.UpdateStatusAsync(
                id, status, userId
            );

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Không thể cập nhật trạng thái tồn kho."
                });
            }

            return Ok(new
            {
                message = "Cập nhật trạng thái tồn kho thành công."
            });
        }
    }
}
