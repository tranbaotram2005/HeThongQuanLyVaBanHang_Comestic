using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    public class ShipperController : ControllerBase
    {
        private readonly WebApiGtEcommerceContext _context;

        public ShipperController(WebApiGtEcommerceContext context)
        {
            _context = context;
        }

        // GET: api/Shipper
        // Lấy danh sách đơn vị vận chuyển đang hoạt động
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var shippers = await _context.Shippers
                .AsNoTracking()
                .Where(s => s.Status == 1)
                .OrderBy(s => s.Id)
                .Select(s => new ShipperResponseDTO
                {
                    Id = s.Id,
                    CompanyName = s.CompanyName,
                    Phone = s.Phone,
                    Notes = s.Notes,
                    Status = s.Status
                })
                .ToListAsync();

            return Ok(shippers);
        }
    }
}
