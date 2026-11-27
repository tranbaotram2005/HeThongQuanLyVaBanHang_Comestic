using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class PaymentController : Controller
    {
        private readonly IPaymentApiService _paymentService;
        private readonly IOrderApiService _orderService;

        public PaymentController(IPaymentApiService paymentService, IOrderApiService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index(int? orderId, int? status)
        {
            var payments = orderId.HasValue
                ? await _paymentService.GetPaymentsByOrderIdAsync(orderId.Value)
                : await _paymentService.GetAllPaymentsAsync();

            if (status.HasValue)
            {
                payments = payments.Where(p => p.Status == status.Value).ToList();
            }

            ViewBag.OrderId = orderId;
            ViewBag.Status = status;
            return View(payments);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();

            ViewBag.Order = await _orderService.GetOrderByIdAsync(payment.OrderId);
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int status, string? notes, string? returnUrl = null)
        {
            var (success, message) = await _paymentService.UpdatePaymentStatusAsync(id, new PaymentUpdateStatusDTO
            {
                Status = status,
                Notes = notes
            });

            TempData[success ? "Success" : "Error"] = message;
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
