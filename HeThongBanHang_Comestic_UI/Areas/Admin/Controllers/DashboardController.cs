using HeThongBanHang_Comestic_UI.Areas.Admin.Models;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class DashboardController : Controller
    {
        private readonly IOrderApiService _orderService;
        private readonly IProductApiService _productService;
        private readonly ICategoryApiService _categoryService;
        private readonly ISupplierApiService _supplierService;

        public DashboardController(
            IOrderApiService orderService,
            IProductApiService productService,
            ICategoryApiService categoryService,
            ISupplierApiService supplierService)
        {
            _orderService = orderService;
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index()
        {
            var ordersTask = _orderService.GetAllOrdersWithStatusAsync();
            var empOrdersTask = _orderService.GetMyEmployeeOrdersAsync();
            var productsTask = _productService.GetAllForAdminAsync();
            var categoriesTask = _categoryService.GetAllForAdminAsync();
            var suppliersTask = _supplierService.GetAllForAdminAsync();

            await Task.WhenAll(ordersTask, empOrdersTask, productsTask, categoriesTask, suppliersTask);

            var (ordersOk, ordersErr, orders) = ordersTask.Result;
            if (!ordersOk && !string.IsNullOrEmpty(ordersErr))
                TempData["Error"] = ordersErr;

            var products = productsTask.Result;
            var categories = categoriesTask.Result;
            var suppliers = suppliersTask.Result;

            var vm = new DashboardVM
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == 0),
                ShippingOrders = orders.Count(o => o.Status == 1),
                CompletedOrders = orders.Count(o => o.Status == 2),
                CancelledOrders = orders.Count(o => o.Status == 3),
                CompletedRevenue = orders.Where(o => o.Status == 2).Sum(o => o.TotalAmount),
                ExpectedRevenue = orders.Where(o => o.Status != 3).Sum(o => o.TotalAmount),

                TotalProducts = products.Count,
                ActiveProducts = products.Count(p => p.Status == 1),
                HiddenProducts = products.Count(p => p.Status == 0),

                TotalCategories = categories.Count,
                ActiveCategories = categories.Count(c => c.Status == 1),

                TotalSuppliers = suppliers.Count,
                ActiveSuppliers = suppliers.Count(s => s.Status == 1),

                MyEmployeeOrdersCount = empOrdersTask.Result.Count,
                IsAdmin = User.IsInRole("Admin"),
                RecentOrders = orders.Take(10).ToList()
            };

            return View(vm);
        }
    }
}
