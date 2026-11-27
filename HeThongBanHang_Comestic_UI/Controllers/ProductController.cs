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
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Employee");
            var products = isStaff
                ? await _productService.GetAllForAdminAsync()
                : (loai.HasValue ? await _productService.GetByCategoryAsync(loai.Value) : await _productService.GetAllAsync());

            if (isStaff && loai.HasValue)
            {
                products = products.Where(p => p.CategoryId == loai.Value).ToList();
            }
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
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Employee");
            var detail = await _variantService.GetProductDetailAsync(id, isStaff);
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
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("Employee");
            var detail = await _variantService.GetProductDetailAsync(id, isStaff);
            if (detail == null)
                return Content("Không tìm thấy sản phẩm");
            return PartialView("_DetailPopup", detail);
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View();
        }

        [Authorize(Roles = "Admin, Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateDTO model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                return View(model);
            }
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(ImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Picture", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await ImageFile.CopyToAsync(stream);
                model.Picture = fileName;
            }
            var (success, message) = await _productService.CreateAsync(model);
            if (!success)
            {
                ModelState.AddModelError("", message);
                ViewBag.Categories = await _categoryService.GetAllAsync();
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = (await _productService.GetAllForAdminAsync()).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            ViewBag.ProductId = id;
            ViewBag.Categories = await _categoryService.GetAllAsync();
            var dto = new ProductUpdateDTO
            {
                ProductName = product.ProductName,
                Brand = product.Brand,
                Description = product.Description,
                QuantityPerUnit = product.QuantityPerUnit,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
                UnitPrice = product.UnitPrice,
                UnitsInStock = product.UnitsInStock,
                Picture = product.Picture,
                Notes = product.Notes
            };
            return View(dto);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductUpdateDTO model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductId = id;
                ViewBag.Categories = await _categoryService.GetAllAsync();
                return View(model);
            }
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(ImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Picture", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await ImageFile.CopyToAsync(stream);
                model.Picture = fileName;
            }
            var (success, message) = await _productService.UpdateAsync(id, model);
            if (!success)
            {
                ModelState.AddModelError("", message);
                ViewBag.ProductId = id;
                ViewBag.Categories = await _categoryService.GetAllAsync();
                return View(model);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = (await _productService.GetAllForAdminAsync()).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // API dùng status = 0 để ẩn sản phẩm (soft delete)
            await _productService.UpdateStatusAsync(id, 0);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id, string? returnUrl = null)
        {
            await _productService.UpdateStatusAsync(id, 0);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Show(int id, string? returnUrl = null)
        {
            await _productService.UpdateStatusAsync(id, 1);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }
        
    }
}
