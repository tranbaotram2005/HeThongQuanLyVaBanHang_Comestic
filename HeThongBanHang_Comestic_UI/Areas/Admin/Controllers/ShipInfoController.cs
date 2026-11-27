using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class ShipInfoController : Controller
    {
        private readonly IShipInfoApiService _shipInfoService;
        private readonly IOrderApiService _orderService;

        public ShipInfoController(IShipInfoApiService shipInfoService, IOrderApiService orderService)
        {
            _shipInfoService = shipInfoService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index(int? orderId, int? status)
        {
            var shipsTask = orderId.HasValue
                ? _shipInfoService.GetShipInfosByOrderIdAsync(orderId.Value)
                : _shipInfoService.GetAllShipInfosAsync();
            var shippersTask = _shipInfoService.GetShippersAsync();

            await Task.WhenAll(shipsTask, shippersTask);

            var ships = shipsTask.Result;
            if (status.HasValue)
            {
                ships = ships.Where(s => s.Status == status.Value).ToList();
            }

            ViewBag.Shippers = shippersTask.Result;
            ViewBag.ShipperMap = shippersTask.Result.ToDictionary(s => s.Id, s => s.CompanyName);
            ViewBag.OrderId = orderId;
            ViewBag.Status = status;
            return View(ships);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var shipTask = _shipInfoService.GetShipInfoByIdAsync(id);
            var shippersTask = _shipInfoService.GetShippersAsync();
            await Task.WhenAll(shipTask, shippersTask);

            var ship = shipTask.Result;
            if (ship == null) return NotFound();

            ViewBag.Shippers = shippersTask.Result;
            ViewBag.Order = await _orderService.GetOrderByIdAsync(ship.OrderId);
            return View(ship);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, ShipInfoUpdateDTO dto, string? returnUrl = null)
        {
            var (success, message) = await _shipInfoService.UpdateShipInfoAsync(id, dto);
            TempData[success ? "Success" : "Error"] = message;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
