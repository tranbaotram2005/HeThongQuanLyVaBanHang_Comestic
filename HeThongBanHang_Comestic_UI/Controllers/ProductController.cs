using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Models;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductApiService _productService;
        private readonly IProductVariantApiService _variantService;
        private readonly IAccountApiService _accountService;
        private readonly ICategoryApiService _categoryService;

        public ProductController(
            IProductApiService productService,
            IProductVariantApiService variantService,
            IAccountApiService accountService,
            ICategoryApiService categoryService)
        {
            _productService = productService;
            _variantService = variantService;
            _accountService = accountService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(
              string? query,
              int? loai,
              int? minPrice,
              int? maxPrice,
              int? supplierId,
              string? sort,
              string? promo,
              int page = 1,
              int record = 9)
        {
            var products = loai.HasValue
                ? await _productService.GetByCategoryAsync(loai.Value)
                : await _productService.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(query))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(p.Brand) && p.Brand.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.SupplierName) && p.SupplierName.Contains(query, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            if (loai.HasValue)
                ViewBag.CurrentLoai = loai;
            if (supplierId.HasValue)
            {
                products = products.Where(p => p.SupplierId == supplierId.Value).ToList();
                ViewBag.SupplierId = supplierId;
            }
            if (minPrice.HasValue)
            {
                products = products.Where(p => (p.UnitPrice ?? 0) >= minPrice.Value).ToList();
                ViewBag.MinPrice = minPrice;
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => (p.UnitPrice ?? 0) <= maxPrice.Value).ToList();
                ViewBag.MaxPrice = maxPrice;
            }
            // Lọc theo chương trình khuyến mãi (ưu tiên cột Notes, kết hợp tên SP & fallback)
            if (!string.IsNullOrWhiteSpace(promo))
            {
                ViewBag.Promo = promo;
                switch (promo.ToLower())
                {
                    case "flashsale":
                        var flashItems = products
                            .Where(p => !string.IsNullOrEmpty(p.Notes) &&
                                        p.Notes.Contains("FlashSale", StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        // Nếu chưa gắn "FlashSale" trong Notes thì fallback lấy giá tốt nhất lên đầu
                        products = flashItems.Any()
                            ? flashItems
                            : products.OrderBy(p => p.UnitPrice ?? decimal.MaxValue).Take(9).ToList();
                        break;
                    case "combo":
                        products = products
                            .Where(p => (!string.IsNullOrEmpty(p.Notes) && p.Notes.Contains("Combo", StringComparison.OrdinalIgnoreCase)) ||
                                        p.ProductName.Contains("Combo", StringComparison.OrdinalIgnoreCase) ||
                                        p.ProductName.Contains("Set", StringComparison.OrdinalIgnoreCase) ||
                                        p.ProductName.Contains("Bộ", StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "freegift":
                        products = products
                            .Where(p => (!string.IsNullOrEmpty(p.Notes) &&
                                         (p.Notes.Contains("FreeGift", StringComparison.OrdinalIgnoreCase) ||
                                          p.Notes.Contains("Quà tặng", StringComparison.OrdinalIgnoreCase))) ||
                                        p.ProductName.Contains("Tặng", StringComparison.OrdinalIgnoreCase) ||
                                        p.ProductName.Contains("Gift", StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                }
            }
            ViewBag.Sort = sort;
            ViewBag.Query = query;
            IEnumerable<DTOs.ProductResponseDTO> ordered = sort == "newest"
                ? products.OrderByDescending(p => p.Id)
                : (promo?.ToLower() == "flashsale" ? products.OrderBy(p => p.UnitPrice ?? decimal.MaxValue) : products.OrderBy(p => p.Id));
            int totalRecord = products.Count;
            int totalPage = (int)Math.Ceiling(totalRecord / (double)record);
            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = page;
            ViewBag.Record = record;
            var result = ordered
                .Skip((page - 1) * record)
                .Take(record)
                .Select(p => new ProductVM
                {
                    productId = p.Id,
                    productName = p.ProductName,
                    productPrice = p.UnitPrice ?? 0,
                    productImage = p.Picture ?? "",
                    Status = p.Status
                })
                .ToList();

            return View(result);
        }

        public IActionResult FilterByCategory(int? loai)
        {
            return RedirectToAction("Index", new { loai = loai });
        }

        public IActionResult FilterByPrice(int? minPrice, int? maxPrice)
        {
            return RedirectToAction("Index", new { minPrice = minPrice, maxPrice = maxPrice });
        }

        public async Task<IActionResult> Search(string? query)
        {
            var products = await _productService.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(query))
            {
                products = products
                    .Where(p => p.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            var result = products.Select(p => new ProductVM
            {
                productId = p.Id,
                productName = p.ProductName,
                productPrice = p.UnitPrice ?? 0,
                productImage = p.Picture ?? ""
            }).ToList();
            return View(result);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            var detail = await _variantService.GetProductDetailAsync(id);
            if (detail == null)
            {
                TempData["Message"] = $"Không thấy sản phẩm có mã {id}";
                return Redirect("/404");
            }
            return View(detail);
        }

        [AllowAnonymous]
        public async Task<IActionResult> DetailPopup(int id)
        {
            var detail = await _variantService.GetProductDetailAsync(id);
            if (detail == null)
                return Content("Không tìm thấy sản phẩm");
            return PartialView("_DetailPopup", detail);
        }

        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create() => RedirectToAction("Create", "Product", new { area = "Admin" });

        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Edit(int id) => RedirectToAction("Edit", "Product", new { area = "Admin", id });

        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Delete(int id) => RedirectToAction("Delete", "Product", new { area = "Admin", id });
    }
}
