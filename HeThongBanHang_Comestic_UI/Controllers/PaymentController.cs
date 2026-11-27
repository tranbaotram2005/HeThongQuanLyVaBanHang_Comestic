using System.Text.Json;
using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize(Roles = "Customer")]
    public class PaymentController : Controller
    {
        private readonly IOrderApiService _orderService;
        private readonly IPaymentApiService _paymentService;

        public PaymentController(
            IOrderApiService orderService,
            IPaymentApiService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> QrPayment(int orderId)
        {
            OrderResponseDTO? order = null;
            if (TempData["QrOrder"] is string json)
            {
                order = JsonSerializer.Deserialize<OrderResponseDTO>(json);
                TempData.Keep("QrOrder");
            }

            order ??= await _orderService.GetMyOrderByIdAsync(orderId);
            if (order == null) return NotFound();

            // Dự phòng nếu load lại trang từ API chưa kèm sẵn nội dung chuyển khoản
            order.TransferContent ??= $"THANHTOAN {order.Id}";
            order.BankAccount ??= "0866975503";
            order.BankName ??= "MB Bank";

            return View(order);
        }

        // Endpoint để JS kiểm tra xem SePay Webhook đã báo thanh toán thành công chưa
        [HttpGet]
        public async Task<IActionResult> CheckStatus(int orderId)
        {
            var payments = await _paymentService.GetMyPaymentsByOrderAsync(orderId);
            bool isPaid = payments.Any(p => p.Status == 1);
            return Json(new { isPaid });
        }

        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _orderService.GetMyOrderByIdAsync(orderId);
            if (order == null) return RedirectToAction("Index", "Home");
            return View(order);
        }
        public IActionResult Failure()
        {
            return View();
        }
    }
}