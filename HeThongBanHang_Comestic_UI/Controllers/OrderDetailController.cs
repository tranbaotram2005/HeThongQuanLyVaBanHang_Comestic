using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize]
    public class OrderDetailController : Controller
    {
        private readonly IOrderApiService _orderService;

        public OrderDetailController(IOrderApiService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> MyOrders()
        {
            var orders = await _orderService.GetMyOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order = await _orderService.GetMyOrderByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var (success, message) = await _orderService.CancelMyOrderAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Detail", new { id });
        }
    }
}