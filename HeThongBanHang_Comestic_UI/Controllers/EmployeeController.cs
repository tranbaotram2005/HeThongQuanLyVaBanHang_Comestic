using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class EmployeeController : Controller
    {
        private readonly IOrderApiService _orderService;

        public EmployeeController(IOrderApiService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index() => RedirectToAction(nameof(Orders));

        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> ApproveOrder(int id, int status = 1)
        {
            var (success, message) = await _orderService.UpdateOrderStatusAsync(id, status);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Orders));
        }
    }
}