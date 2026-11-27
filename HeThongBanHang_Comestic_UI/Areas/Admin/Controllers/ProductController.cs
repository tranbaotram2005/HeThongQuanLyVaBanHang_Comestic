using HeThongBanHang_Comestic_UI.Areas.Admin.Models;
using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class ProductController : Controller
    {
        private readonly IProductApiService _productService;
        private readonly IProductVariantApiService _variantService;
        private readonly ICategoryApiService _categoryService;
        private readonly ISupplierApiService _supplierService;
        private readonly IOptionPresetApiService _optionPresetService;
        private readonly IWebHostEnvironment _env;

        public ProductController(
            IProductApiService productService,
            IProductVariantApiService variantService,
            ICategoryApiService categoryService,
            ISupplierApiService supplierService,
            IOptionPresetApiService optionPresetService,
            IWebHostEnvironment env)
        {
            _productService = productService;
            _variantService = variantService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _optionPresetService = optionPresetService;
            _env = env;
        }

        public async Task<IActionResult> Index(string? query)
        {
            var products = await _productService.GetAllForAdminAsync();
            if (!string.IsNullOrWhiteSpace(query))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(p.CategoryName) && p.CategoryName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.SupplierName) && p.SupplierName.Contains(query, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ViewBag.Query = query;
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View(new ProductCreateWithVariantsVM
            {
                Variants = new List<ProductVariantFormItem>
                {
                    new() { IsDefault = true, Status = 1, OriginalStatus = 1 }
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateWithVariantsVM model)
        {
            model.Variants ??= new List<ProductVariantFormItem>();

            if (!ModelState.IsValid)
            {
                if (!model.Variants.Any())
                    model.Variants.Add(new ProductVariantFormItem { IsDefault = true, Status = 1, OriginalStatus = 1 });
                await LoadDropdownsAsync();
                return View(model);
            }

            var uploaded = await SaveImageAsync(model.ImageFile, "Picture");
            if (!string.IsNullOrEmpty(uploaded))
                model.Picture = uploaded;

            var activeVariants = model.Variants
                .Select((v, index) => (Variant: v, Index: index))
                .Where(x => IsVariantPopulated(x.Variant))
                .ToList();

            var defaultPrice = model.UnitPrice
                ?? activeVariants.FirstOrDefault(x => x.Variant.IsDefault && x.Variant.Price.HasValue).Variant?.Price
                ?? activeVariants.FirstOrDefault(x => x.Variant.Price.HasValue).Variant?.Price;

            short? totalStock = model.UnitsInStock;
            if (!totalStock.HasValue && activeVariants.Any())
            {
                var summed = activeVariants.Sum(x => x.Variant.StockQuantity ?? 0);
                totalStock = (short)Math.Clamp(summed, 0, short.MaxValue);
            }

            var productDto = new ProductCreateDTO
            {
                ProductName = model.ProductName,
                Brand = model.Brand,
                Description = model.Description,
                QuantityPerUnit = model.QuantityPerUnit,
                CategoryId = model.CategoryId,
                SupplierId = model.SupplierId,
                UnitPrice = defaultPrice,
                UnitsInStock = totalStock,
                Picture = model.Picture,
                Notes = model.Notes
            };

            var (success, message, newProductId) = await _productService.CreateAsync(productDto);
            if (!success)
            {
                ModelState.AddModelError("", message);
                if (!model.Variants.Any())
                    model.Variants.Add(new ProductVariantFormItem { IsDefault = true, Status = 1, OriginalStatus = 1 });
                await LoadDropdownsAsync();
                return View(model);
            }

            var variantErrors = new List<string>();
            var syncedVariants = new List<(string Sku, ProductVariantFormItem Variant)>();

            if (newProductId.HasValue && activeVariants.Any())
            {
                NormalizeDefaultFlag(activeVariants.Select(x => x.Variant).ToList());

                foreach (var (variant, idx) in activeVariants)
                {
                    var variantImg = await SaveImageAsync(Request.Form.Files[$"VariantFile_{idx}"], "ProductVariant");
                    var sku = !string.IsNullOrWhiteSpace(variant.Sku)
                        ? variant.Sku.Trim()
                        : GenerateSku(newProductId.Value, idx);
                    var variantName = ResolveVariantName(variant, model.ProductName);

                    var createDto = new ProductVariantCreateDTO
                    {
                        ProductId = newProductId.Value,
                        Sku = sku,
                        Name = variantName,
                        Description = variant.Description,
                        Barcode = variant.Barcode,
                        Price = variant.Price ?? defaultPrice ?? 0,
                        CompareAtPrice = variant.CompareAtPrice,
                        StockQuantity = variant.StockQuantity ?? 0,
                        ImageUrl = variantImg ?? variant.ImageUrl,
                        IsDefault = variant.IsDefault,
                        Weight = variant.Weight,
                        Length = variant.Length,
                        Width = variant.Width,
                        Height = variant.Height
                    };

                    var (vOk, vMsg) = await _variantService.CreateAsync(createDto);
                    if (!vOk)
                        variantErrors.Add($"Biến thể \"{variantName}\": {vMsg}");
                    else
                        syncedVariants.Add((sku, variant));
                }

                if (syncedVariants.Any() && (!string.IsNullOrWhiteSpace(model.Option1Name) || !string.IsNullOrWhiteSpace(model.Option2Name)))
                {
                    var syncDto = BuildOptionsSyncDto(model.Option1Name, model.Option2Name, syncedVariants);
                    var (optOk, optMsg) = await _variantService.SyncOptionsAsync(newProductId.Value, syncDto);
                    if (!optOk)
                        variantErrors.Add($"Thuộc tính sản phẩm: {optMsg}");
                }
            }
            else if (!newProductId.HasValue && activeVariants.Any())
            {
                variantErrors.Add("Không lấy được ID sản phẩm mới nên chưa thể tạo biến thể.");
            }

            TempData["Success"] = "Thêm sản phẩm thành công.";
            if (variantErrors.Any())
                TempData["Error"] = string.Join(" | ", variantErrors);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = (await _productService.GetAllForAdminAsync()).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var variants = await _variantService.GetByProductForAdminAsync(id);
            var (optionNames, variantOptionMap) = await _variantService.GetAdminOptionsByProductAsync(id, variants.Select(v => v.Id));
            var opt1Name = optionNames.ElementAtOrDefault(0);
            var opt2Name = optionNames.ElementAtOrDefault(1);

            await LoadDropdownsAsync();

            var vm = new ProductEditWithVariantsVM
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Brand = product.Brand,
                Description = product.Description,
                QuantityPerUnit = product.QuantityPerUnit,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
                UnitPrice = product.UnitPrice,
                UnitsInStock = product.UnitsInStock,
                Picture = product.Picture,
                Notes = product.Notes,
                Option1Name = opt1Name,
                Option2Name = opt2Name,
                Variants = variants.Select(v =>
                {
                    variantOptionMap.TryGetValue(v.Id, out var opts);
                    string? vOpt1 = null;
                    string? vOpt2 = null;
                    if (opts != null)
                    {
                        if (!string.IsNullOrEmpty(opt1Name)) opts.TryGetValue(opt1Name, out vOpt1);
                        if (!string.IsNullOrEmpty(opt2Name)) opts.TryGetValue(opt2Name, out vOpt2);
                    }

                    return new ProductVariantFormItem
                    {
                        Id = v.Id,
                        Sku = v.Sku,
                        Name = v.Name,
                        Option1Value = vOpt1,
                        Option2Value = vOpt2,
                        Description = v.Description,
                        Barcode = v.Barcode,
                        Price = v.Price,
                        CompareAtPrice = v.CompareAtPrice,
                        StockQuantity = v.StockQuantity,
                        ImageUrl = v.ImageUrl,
                        IsDefault = v.IsDefault ?? false,
                        Weight = v.Weight,
                        Length = v.Length,
                        Width = v.Width,
                        Height = v.Height,
                        Status = v.Status ?? 1,
                        OriginalStatus = v.Status ?? 1
                    };
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditWithVariantsVM model)
        {
            model.Id = id;
            model.Variants ??= new List<ProductVariantFormItem>();

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            var uploaded = await SaveImageAsync(model.ImageFile, "Picture");
            if (!string.IsNullOrEmpty(uploaded))
                model.Picture = uploaded;

            var productDto = new ProductUpdateDTO
            {
                ProductName = model.ProductName,
                Brand = model.Brand,
                Description = model.Description,
                QuantityPerUnit = model.QuantityPerUnit,
                CategoryId = model.CategoryId,
                SupplierId = model.SupplierId,
                UnitPrice = model.UnitPrice,
                UnitsInStock = model.UnitsInStock,
                Picture = model.Picture,
                Notes = model.Notes
            };

            var (success, message) = await _productService.UpdateAsync(id, productDto);
            if (!success)
            {
                ModelState.AddModelError("", message);
                await LoadDropdownsAsync();
                return View(model);
            }

            var populatedVariants = model.Variants
                .Select((v, index) => (Variant: v, Index: index))
                .Where(x => x.Variant.Id > 0 || IsVariantPopulated(x.Variant))
                .ToList();

            if (populatedVariants.Any())
                NormalizeDefaultFlag(populatedVariants.Select(x => x.Variant).ToList());

            var variantErrors = new List<string>();
            var syncedVariants = new List<(string Sku, ProductVariantFormItem Variant)>();

            foreach (var (variant, idx) in populatedVariants)
            {
                var variantImg = await SaveImageAsync(Request.Form.Files[$"VariantFile_{idx}"], "ProductVariant");
                if (!string.IsNullOrEmpty(variantImg))
                    variant.ImageUrl = variantImg;

                var sku = !string.IsNullOrWhiteSpace(variant.Sku) ? variant.Sku.Trim() : GenerateSku(id, idx);
                var variantName = ResolveVariantName(variant, model.ProductName);

                if (variant.Id > 0)
                {
                    var updateDto = new ProductVariantUpdateDTO
                    {
                        Sku = sku,
                        Name = variantName,
                        Description = variant.Description,
                        Barcode = variant.Barcode,
                        Price = variant.Price ?? model.UnitPrice ?? 0,
                        CompareAtPrice = variant.CompareAtPrice,
                        StockQuantity = variant.StockQuantity ?? 0,
                        ImageUrl = variant.ImageUrl,
                        IsDefault = variant.IsDefault,
                        Weight = variant.Weight,
                        Length = variant.Length,
                        Width = variant.Width,
                        Height = variant.Height
                    };

                    var (vOk, vMsg) = await _variantService.UpdateAsync(variant.Id, updateDto);
                    if (!vOk)
                    {
                        variantErrors.Add($"Biến thể \"{variantName}\": {vMsg}");
                        continue;
                    }

                    if (variant.Status != variant.OriginalStatus && (variant.Status == 0 || variant.Status == 1))
                    {
                        var (sOk, sMsg) = await _variantService.UpdateStatusAsync(variant.Id, variant.Status);
                        if (!sOk)
                            variantErrors.Add($"Trạng thái \"{variantName}\": {sMsg}");
                    }

                    syncedVariants.Add((sku, variant));
                }
                else
                {
                    var createDto = new ProductVariantCreateDTO
                    {
                        ProductId = id,
                        Sku = sku,
                        Name = variantName,
                        Description = variant.Description,
                        Barcode = variant.Barcode,
                        Price = variant.Price ?? model.UnitPrice ?? 0,
                        CompareAtPrice = variant.CompareAtPrice,
                        StockQuantity = variant.StockQuantity ?? 0,
                        ImageUrl = variant.ImageUrl,
                        IsDefault = variant.IsDefault,
                        Weight = variant.Weight,
                        Length = variant.Length,
                        Width = variant.Width,
                        Height = variant.Height
                    };

                    var (vOk, vMsg) = await _variantService.CreateAsync(createDto);
                    if (!vOk)
                        variantErrors.Add($"Thêm biến thể \"{variantName}\": {vMsg}");
                    else
                        syncedVariants.Add((sku, variant));
                }
            }

            if (syncedVariants.Any())
            {
                var syncDto = BuildOptionsSyncDto(model.Option1Name, model.Option2Name, syncedVariants);
                var (optOk, optMsg) = await _variantService.SyncOptionsAsync(id, syncDto);
                if (!optOk)
                    variantErrors.Add($"Thuộc tính sản phẩm: {optMsg}");
            }

            TempData["Success"] = "Cập nhật sản phẩm thành công.";
            if (variantErrors.Any())
                TempData["Error"] = string.Join(" | ", variantErrors);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var (success, message) = await _productService.UpdateStatusAsync(id, 0);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Show(int id)
        {
            var (success, message) = await _productService.UpdateStatusAsync(id, 1);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = (await _productService.GetAllForAdminAsync()).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _productService.UpdateStatusAsync(id, 0);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            var catsTask = _categoryService.GetAllAsync();
            var suppsTask = _supplierService.GetAllAsync();
            var presetsTask = _optionPresetService.GetActiveAsync();
            await Task.WhenAll(catsTask, suppsTask, presetsTask);
            ViewBag.Categories = catsTask.Result;
            ViewBag.Suppliers = suppsTask.Result;
            ViewBag.OptionPresets = presetsTask.Result;
        }

        private async Task<string?> SaveImageAsync(IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return null;
            var folder = Path.Combine(_env.WebRootPath, subfolder);
            Directory.CreateDirectory(folder);
            var safeName = Path.GetFileName(file.FileName);
            var fileName = subfolder == "Picture"
                ? safeName
                : $"{Guid.NewGuid().ToString("N")[..8]}_{safeName}";
            using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }

        private static bool IsVariantPopulated(ProductVariantFormItem v)
        {
            return !string.IsNullOrWhiteSpace(v.Name)
                || !string.IsNullOrWhiteSpace(v.Option1Value)
                || !string.IsNullOrWhiteSpace(v.Option2Value)
                || !string.IsNullOrWhiteSpace(v.Sku)
                || v.Price.HasValue
                || v.CompareAtPrice.HasValue
                || v.StockQuantity.HasValue
                || !string.IsNullOrWhiteSpace(v.ImageUrl);
        }

        private static string ResolveVariantName(ProductVariantFormItem v, string fallbackProductName)
        {
            if (!string.IsNullOrWhiteSpace(v.Name))
                return v.Name.Trim();

            var parts = new[] { v.Option1Value?.Trim(), v.Option2Value?.Trim() }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            return parts.Any() ? string.Join(" - ", parts) : fallbackProductName;
        }

        private static ProductOptionsSyncDTO BuildOptionsSyncDto(
            string? option1Name,
            string? option2Name,
            List<(string Sku, ProductVariantFormItem Variant)> variants)
        {
            var dto = new ProductOptionsSyncDTO();
            var opt1 = option1Name?.Trim();
            var opt2 = option2Name?.Trim();

            if (!string.IsNullOrWhiteSpace(opt1)) dto.OptionNames.Add(opt1);
            if (!string.IsNullOrWhiteSpace(opt2) && !string.Equals(opt2, opt1, StringComparison.OrdinalIgnoreCase))
                dto.OptionNames.Add(opt2);

            foreach (var (sku, v) in variants)
            {
                var item = new VariantOptionsSyncItemDTO { Sku = sku };
                if (!string.IsNullOrWhiteSpace(opt1) && !string.IsNullOrWhiteSpace(v.Option1Value))
                    item.Options.Add(new VariantOptionValuePairDTO { OptionName = opt1, OptionValue = v.Option1Value.Trim() });

                if (!string.IsNullOrWhiteSpace(opt2) && !string.Equals(opt2, opt1, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(v.Option2Value))
                    item.Options.Add(new VariantOptionValuePairDTO { OptionName = opt2, OptionValue = v.Option2Value.Trim() });

                dto.Variants.Add(item);
            }

            return dto;
        }

        private static string GenerateSku(int productId, int index)
        {
            return $"SP{productId}-V{index + 1}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
        }

        private static void NormalizeDefaultFlag(List<ProductVariantFormItem> variants)
        {
            if (!variants.Any()) return;
            var firstDefaultIndex = variants.FindIndex(v => v.IsDefault);
            if (firstDefaultIndex < 0) firstDefaultIndex = 0;

            for (int i = 0; i < variants.Count; i++)
            {
                variants[i].IsDefault = (i == firstDefaultIndex);
            }
        }
    }
}
