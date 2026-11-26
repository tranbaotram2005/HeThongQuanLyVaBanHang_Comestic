
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShipInfoController : ControllerBase
    {
        private readonly IShipInfoService _shipInfoService;

        public ShipInfoController(IShipInfoService shipInfoService)
        {
            _shipInfoService = shipInfoService;
        }

        // Lấy UserId từ JWT
        private int GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) ||
                !int.TryParse(userId, out int id))
            {
                throw new UnauthorizedAccessException(
                    "Không xác định được người dùng.");
            }

            return id;
        }

        // =====================================================
        // 1. CUSTOMER - XEM THÔNG TIN GIAO HÀNG ĐƠN MÌNH
        // GET: api/ShipInfo/my/order/5
        // =====================================================
        [HttpGet("my/order/{orderId:int}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyShipInfos(int orderId)
        {
            int userId = GetUserId();

            var result = await _shipInfoService.GetMyShipInfosAsync(
                userId, orderId);

            return Ok(result);
        }

        // =====================================================
        // 2. ADMIN, EMPLOYEE - XEM THEO ORDER ID
        // GET: api/ShipInfo/order/5
        // =====================================================
        [HttpGet("order/{orderId:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetShipInfosByOrderId(int orderId)
        {
            var result = await _shipInfoService
                .GetShipInfosByOrderIdAsync(orderId);

            return Ok(result);
        }

        // =====================================================
        // 3. ADMIN, EMPLOYEE - XEM TẤT CẢ
        // GET: api/ShipInfo
        // =====================================================
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetAllShipInfos()
        {
            var result = await _shipInfoService.GetAllShipInfosAsync();

            return Ok(result);
        }

        // =====================================================
        // 4. ADMIN, EMPLOYEE - XEM CHI TIẾT
        // GET: api/ShipInfo/5
        // =====================================================
        [HttpGet("{shipInfoId:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetShipInfoById(int shipInfoId)
        {
            var result = await _shipInfoService.GetShipInfoByIdAsync(
                shipInfoId);

            if (result == null)
                return NotFound("Không tìm thấy thông tin giao hàng.");

            return Ok(result);
        }

        // =====================================================
        // 5. ADMIN, EMPLOYEE - CẬP NHẬT
        // PUT: api/ShipInfo/5
        // =====================================================
        [HttpPut("{shipInfoId:int}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateShipInfo(
            int shipInfoId,
            ShipInfoUpdateDTO dto)
        {
            try
            {
                int userId = GetUserId();

                bool result = await _shipInfoService.UpdateShipInfoAsync(
                    shipInfoId, dto, userId);

                if (!result)
                    return NotFound("Không tìm thấy thông tin giao hàng.");

                return Ok(new
                {
                    message = "Cập nhật thông tin giao hàng thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
