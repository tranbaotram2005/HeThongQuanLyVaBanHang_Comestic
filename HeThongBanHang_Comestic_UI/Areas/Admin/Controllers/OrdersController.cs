using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class OrdersController : Controller
    {
        private readonly IOrderApiService _orderService;
        private readonly IProductApiService _productService;
        private readonly IProductVariantApiService _variantService;
        private readonly IPaymentApiService _paymentService;
        private readonly IShipInfoApiService _shipInfoService;

        public OrdersController(
            IOrderApiService orderService,
            IProductApiService productService,
            IProductVariantApiService variantService,
            IPaymentApiService paymentService,
            IShipInfoApiService shipInfoService)
        {
            _orderService = orderService;
            _productService = productService;
            _variantService = variantService;
            _paymentService = paymentService;
            _shipInfoService = shipInfoService;
        }

        public async Task<IActionResult> Index(string? mode)
        {
            bool isEmployeeOnly = !User.IsInRole("Admin") && User.IsInRole("Employee");
            string activeMode = string.IsNullOrEmpty(mode)
                ? (isEmployeeOnly ? "mine" : "all")
                : mode.ToLower();

            List<OrderResponseDTO> orders;
            if (activeMode == "mine")
            {
                orders = await _orderService.GetMyEmployeeOrdersAsync();
                ViewBag.PageTitle = "Đơn hàng tôi đặt dùm khách";
            }
            else
            {
                var (ok, err, list) = await _orderService.GetAllOrdersWithStatusAsync();
                orders = list;
                if (!ok && !string.IsNullOrEmpty(err))
                    TempData["Error"] = err;
                ViewBag.PageTitle = "Quản lý tất cả đơn hàng";
            }

            ViewBag.ActiveMode = activeMode;
            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var orderTask = _orderService.GetOrderByIdAsync(id);
            var payTask = _paymentService.GetPaymentsByOrderIdAsync(id);
            var shipTask = _shipInfoService.GetShipInfosByOrderIdAsync(id);
            var shippersTask = _shipInfoService.GetShippersAsync();

            await Task.WhenAll(orderTask, payTask, shipTask, shippersTask);

            var order = orderTask.Result;
            if (order == null) return NotFound();

            ViewBag.Payments = payTask.Result;
            ViewBag.ShipInfos = shipTask.Result;
            ViewBag.Shippers = shippersTask.Result;
            return View(order);
        }

        public async Task<IActionResult> ApproveOrder(int id, int status = 1, string? returnUrl = null)
        {
            var (success, message) = await _orderService.UpdateOrderStatusAsync(id, status);
            TempData[success ? "Success" : "Error"] = message;
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrder()
        {
            await LoadCreateOrderDropdownsAsync();
            return View(new EmployeeOrderCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(EmployeeOrderCreateDTO dto)
        {
            dto.Items = dto.Items?
                .Where(x => x.ProductVariantId > 0 && x.Quantity > 0)
                .ToList() ?? new();

            if (!dto.Items.Any())
                ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 sản phẩm và số lượng hợp lệ.");

            if (!ModelState.IsValid)
            {
                await LoadCreateOrderDropdownsAsync();
                return View(dto);
            }

            var (success, message, order) = await _orderService.CreateOrderByEmployeeAsync(dto);
            if (!success || order == null)
            {
                ModelState.AddModelError("", message);
                await LoadCreateOrderDropdownsAsync();
                return View(dto);
            }

            TempData["Success"] = $"Đã tạo đơn hàng #{order.Id} dùm khách thành công.";
            return RedirectToAction(nameof(Detail), new { id = order.Id });
        }

        private async Task LoadCreateOrderDropdownsAsync()
        {
            var customersTask = _orderService.GetCustomersAsync();
            var productsTask = _productService.GetAllAsync();
            await Task.WhenAll(customersTask, productsTask);

            var products = productsTask.Result;
            var variantOptions = new List<object>();

            foreach (var chunk in products.Chunk(15))
            {
                var batch = await Task.WhenAll(
                    chunk.Select(async p => (Product: p, Variants: await _variantService.GetByProductAsync(p.Id)))
                );

                foreach (var entry in batch)
                {
                    foreach (var v in entry.Variants.Where(v => (v.StockQuantity ?? 0) > 0))
                    {
                        decimal price = v.Price ?? entry.Product.UnitPrice ?? 0;
                        variantOptions.Add(new
                        {
                            Id = v.Id,
                            Price = price,
                            Stock = v.StockQuantity ?? 0,
                            Label = $"{entry.Product.ProductName} - {v.Name} ({price:N0} đ | Kho: {v.StockQuantity ?? 0})"
                        });
                    }
                }
            }

            ViewBag.Customers = customersTask.Result;
            ViewBag.VariantOptions = variantOptions;
        }
    }
}
