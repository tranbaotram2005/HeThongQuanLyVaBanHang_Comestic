using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize]
    public class OrderDetailController : Controller
    {
        private readonly IOrderApiService _orderService;
        private readonly IPaymentApiService _paymentService;
        private readonly IShipInfoApiService _shipInfoService;

        public OrderDetailController(
            IOrderApiService orderService,
            IPaymentApiService paymentService,
            IShipInfoApiService shipInfoService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _shipInfoService = shipInfoService;
        }

        public async Task<IActionResult> MyOrders()
        {
            List<OrderResponseDTO> orders;

            if (User.IsInRole("Admin"))
            {
                var (ok, err, list) = await _orderService.GetAllOrdersWithStatusAsync();
                orders = list;
                if (!ok && !string.IsNullOrEmpty(err))
                    TempData["Error"] = err;
                ViewBag.PageTitle = "Lịch sử tất cả đơn hàng (Admin)";
                ViewBag.RoleMode = "Admin";
            }
            else if (User.IsInRole("Employee"))
            {
                orders = await _orderService.GetMyEmployeeOrdersAsync();
                ViewBag.PageTitle = "Đơn hàng tôi đặt dùm khách";
                ViewBag.RoleMode = "Employee";
            }
            else
            {
                orders = await _orderService.GetMyOrdersAsync();
                ViewBag.PageTitle = "Lịch sử đặt hàng của tôi";
                ViewBag.RoleMode = "Customer";
            }

            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Employee");
            var order = isStaff
                ? await _orderService.GetOrderByIdAsync(id)
                : await _orderService.GetMyOrderByIdAsync(id);

            if (order == null) return NotFound();

            if (isStaff)
            {
                ViewBag.MyPayments = await _paymentService.GetPaymentsByOrderIdAsync(id);
                ViewBag.MyShipInfos = await _shipInfoService.GetShipInfosByOrderIdAsync(id);
            }
            else
            {
                ViewBag.MyPayments = await _paymentService.GetMyPaymentsByOrderAsync(id);
                ViewBag.MyShipInfos = await _shipInfoService.GetMyShipInfosByOrderAsync(id);
            }

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
