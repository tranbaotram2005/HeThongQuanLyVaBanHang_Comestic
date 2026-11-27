using HeThongBanHang_Comestic_UI.Models;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryApiService _categoryService;
        private readonly IProductApiService _productService;
        private readonly IProductVariantApiService _variantService;
        private readonly ISupplierApiService _supplierService;

        public HomeController(
            ILogger<HomeController> logger,
            ICategoryApiService categoryService,
            IProductApiService productService,
            ISupplierApiService supplierService,
            IProductVariantApiService variantService)
        {
            _logger = logger;
            _categoryService = categoryService;
            _productService = productService;
            _variantService = variantService;
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index()
        {
            // Gọi song song cả 2 API
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Employee");
            var categoriesTask = isStaff ? _categoryService.GetAllForAdminAsync() : _categoryService.GetAllAsync();
            var productsTask = isStaff ? _productService.GetAllForAdminAsync() : _productService.GetAllAsync();
            var suppliersTask = isStaff ? _supplierService.GetAllForAdminAsync() : _supplierService.GetAllAsync();
            await Task.WhenAll(categoriesTask, productsTask, suppliersTask);
            var categories = categoriesTask.Result;
            var allProducts = productsTask.Result;

            // Tự đếm số sản phẩm cho từng danh mục
            foreach (var c in categories)
            {
                c.ProductCount = allProducts.Count(p => p.CategoryId == c.Id);
            }
            var viewModel = new HomeViewModels
            {
                Categories = categories,
                Products = (isStaff
                    ? allProducts.OrderBy(p => p.Status).ThenBy(_ => Guid.NewGuid())
                    : allProducts.OrderBy(_ => Guid.NewGuid())).Take(8).ToList(),
                Suppliers = suppliersTask.Result
            };
            return View(viewModel);
        }

        [Route("/404")]
        public IActionResult PageNotFound()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> GetProductDetail(int id)
        {
            var detail = await _variantService.GetProductDetailAsync(id);
            if (detail == null) return NotFound();
            return PartialView("~/Views/Product/_DetailPopup.cshtml", detail);
        }

    }
}
