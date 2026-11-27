
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public ProductVariantService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy danh sách biến thể theo sản phẩm
        public async Task<List<ProductVariantResponseDTO>> GetByProductAsync(int productId)
        {
            var variants = await _context.ProductVariants
                .Where(x => x.ProductId == productId && x.Status == 1)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Id)
                .ToListAsync();

            return _mapper.Map<List<ProductVariantResponseDTO>>(variants);
        }

        // Admin / Employee: lấy toàn bộ biến thể (kể cả đã ẩn)
        public async Task<List<ProductVariantResponseDTO>> GetByProductForAdminAsync(int productId)
        {
            var variants = await _context.ProductVariants
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.Status)
                .ThenByDescending(x => x.IsDefault)
                .ThenBy(x => x.Id)
                .ToListAsync();

            return _mapper.Map<List<ProductVariantResponseDTO>>(variants);
        }

        // Lấy chi tiết một biến thể
        public async Task<ProductVariantResponseDTO?> GetByIdAsync(int id)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == id && x.Status == 1);

            return variant == null
                ? null
                : _mapper.Map<ProductVariantResponseDTO>(variant);
        }

        // Thêm biến thể mới
        public async Task<bool> CreateAsync(ProductVariantCreateDTO dto, int userId)
        {
            // Kiểm tra sản phẩm có tồn tại và đang hoạt động không
            bool productExists = await _context.Products
                .AnyAsync(x => x.Id == dto.ProductId && x.Status == 1);

            if (!productExists)
                return false;

            // SKU bắt buộc và không được trùng
            if (string.IsNullOrWhiteSpace(dto.Sku))
                return false;

            bool skuExists = await _context.ProductVariants
                .AnyAsync(x => x.Sku == dto.Sku);

            if (skuExists)
                return false;

            // Kiểm tra giá trị không được âm
            if (dto.Price < 0 ||
                dto.CompareAtPrice < 0 ||
                dto.StockQuantity < 0 ||
                dto.Weight < 0 ||
                dto.Length < 0 ||
                dto.Width < 0 ||
                dto.Height < 0)
            {
                return false;
            }

            var variant = _mapper.Map<ProductVariant>(dto);

            variant.CreatedAt = DateTime.Now;
            variant.CreatedBy = userId;
            variant.Status = 1;

            // Nếu biến thể mới là mặc định,
            // bỏ mặc định của các biến thể khác cùng sản phẩm
            if (variant.IsDefault == true)
            {
                var oldDefaults = await _context.ProductVariants
                    .Where(x => x.ProductId == dto.ProductId
                             && x.IsDefault == true
                             && x.Status == 1)
                    .ToListAsync();

                foreach (var item in oldDefaults)
                {
                    item.IsDefault = false;
                    item.UpdatedAt = DateTime.Now;
                    item.UpdatedBy = userId;
                }
            }

            await _context.ProductVariants.AddAsync(variant);
            await _context.SaveChangesAsync();

            await SyncVariantInventoryAsync(variant.Id, variant.StockQuantity ?? 0, userId);

            return true;
        }

        // Cập nhật biến thể
        public async Task<bool> UpdateAsync(int id, ProductVariantUpdateDTO dto, int userId)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == id);

            if (variant == null)
                return false;

            // SKU bắt buộc và không được trùng với biến thể khác
            if (string.IsNullOrWhiteSpace(dto.Sku))
                return false;

            bool skuExists = await _context.ProductVariants
                .AnyAsync(x => x.Sku == dto.Sku && x.Id != id);

            if (skuExists)
                return false;

            // Kiểm tra giá trị không được âm
            if (dto.Price < 0 ||
                dto.CompareAtPrice < 0 ||
                dto.StockQuantity < 0 ||
                dto.Weight < 0 ||
                dto.Length < 0 ||
                dto.Width < 0 ||
                dto.Height < 0)
            {
                return false;
            }

            _mapper.Map(dto, variant);

            variant.UpdatedAt = DateTime.Now;
            variant.UpdatedBy = userId;

            // Nếu biến thể này được chọn làm mặc định,
            // bỏ mặc định của các biến thể khác cùng sản phẩm
            if (variant.IsDefault == true)
            {
                var oldDefaults = await _context.ProductVariants
                    .Where(x => x.ProductId == variant.ProductId
                             && x.Id != id
                             && x.IsDefault == true
                             && x.Status == 1)
                    .ToListAsync();

                foreach (var item in oldDefaults)
                {
                    item.IsDefault = false;
                    item.UpdatedAt = DateTime.Now;
                    item.UpdatedBy = userId;
                }
            }

            await _context.SaveChangesAsync();

            if (dto.StockQuantity.HasValue)
            {
                await SyncVariantInventoryAsync(variant.Id, dto.StockQuantity.Value, userId);
            }

            return true;
        }

        private async Task SyncVariantInventoryAsync(int variantId, int targetStock, int userId)
        {
            if (targetStock < 0) targetStock = 0;

            var inventories = await _context.Inventories
                .Where(i => i.VariantId == variantId && i.Status == 1)
                .OrderBy(i => i.Id)
                .ToListAsync();

            if (!inventories.Any())
            {
                if (targetStock > 0)
                {
                    _context.Inventories.Add(new Inventory
                    {
                        VariantId = variantId,
                        Quantity = targetStock,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                        Notes = "Đồng bộ tồn kho từ biến thể",
                        Status = 1
                    });
                    await _context.SaveChangesAsync();
                }
                return;
            }

            int otherWarehousesSum = inventories.Skip(1).Sum(i => i.Quantity ?? 0);
            int firstQty = Math.Max(0, targetStock - otherWarehousesSum);
            inventories[0].Quantity = firstQty;
            inventories[0].UpdatedAt = DateTime.Now;
            inventories[0].UpdatedBy = userId;
            await _context.SaveChangesAsync();
        }

        // Ẩn hoặc kích hoạt lại biến thể
        public async Task<bool> UpdateStatusAsync(int id, int status, int userId)
        {
            if (status != 0 && status != 1)
                return false;

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == id);

            if (variant == null)
                return false;

            variant.Status = status;
            variant.UpdatedAt = DateTime.Now;
            variant.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }

        // Đồng bộ nhóm thuộc tính (VD: Dung tích, Mùi hương) và gán giá trị thuộc tính cho các biến thể
        public async Task<bool> SyncOptionsAsync(int productId, ProductOptionsSyncDTO dto, int userId)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
                return false;

            var cleanOptionNames = (dto.OptionNames ?? new List<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var variants = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            var variantBySku = variants
                .Where(v => !string.IsNullOrWhiteSpace(v.Sku))
                .GroupBy(v => v.Sku!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var existingOptions = await _context.ProductOptions
                .Include(o => o.ProductOptionValues)
                .Where(o => o.ProductId == productId)
                .ToListAsync();

            var optionMap = new Dictionary<string, ProductOption>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < cleanOptionNames.Count; i++)
            {
                var optName = cleanOptionNames[i];
                var existingOpt = existingOptions.FirstOrDefault(o =>
                    o.Name.Trim().Equals(optName, StringComparison.OrdinalIgnoreCase));

                if (existingOpt == null)
                {
                    existingOpt = new ProductOption
                    {
                        ProductId = productId,
                        Name = optName,
                        SortOrder = i + 1,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                        Status = 1
                    };
                    _context.ProductOptions.Add(existingOpt);
                    await _context.SaveChangesAsync();
                    existingOptions.Add(existingOpt);
                }
                else
                {
                    existingOpt.Name = optName;
                    existingOpt.SortOrder = i + 1;
                    existingOpt.Status = 1;
                    existingOpt.UpdatedAt = DateTime.Now;
                    existingOpt.UpdatedBy = userId;
                }

                optionMap[optName] = existingOpt;
            }

            foreach (var oldOpt in existingOptions)
            {
                if (!optionMap.ContainsKey(oldOpt.Name.Trim()))
                {
                    oldOpt.Status = 0;
                    oldOpt.UpdatedAt = DateTime.Now;
                    oldOpt.UpdatedBy = userId;
                }
            }

            await _context.SaveChangesAsync();

            var variantIds = variants.Select(v => v.Id).ToList();
            var allVariantLinks = await _context.ProductVariantOptionValues
                .Where(x => variantIds.Contains(x.ProductVariantId))
                .ToListAsync();

            var usedValueIds = new HashSet<int>();
            var optionSortCounters = new Dictionary<int, int>();

            foreach (var variantItem in dto.Variants ?? new List<VariantOptionsSyncItemDTO>())
            {
                if (string.IsNullOrWhiteSpace(variantItem.Sku))
                    continue;

                if (!variantBySku.TryGetValue(variantItem.Sku.Trim(), out var dbVariant))
                    continue;

                var targetValueIds = new HashSet<int>();

                foreach (var pair in variantItem.Options ?? new List<VariantOptionValuePairDTO>())
                {
                    if (string.IsNullOrWhiteSpace(pair.OptionName) || string.IsNullOrWhiteSpace(pair.OptionValue))
                        continue;

                    var optName = pair.OptionName.Trim();
                    var valName = pair.OptionValue.Trim();

                    if (!optionMap.TryGetValue(optName, out var parentOption))
                        continue;

                    var existingVal = parentOption.ProductOptionValues.FirstOrDefault(v =>
                        v.Name.Trim().Equals(valName, StringComparison.OrdinalIgnoreCase));

                    if (existingVal == null)
                    {
                        optionSortCounters.TryGetValue(parentOption.Id, out int currentOrder);
                        currentOrder++;
                        optionSortCounters[parentOption.Id] = currentOrder;

                        existingVal = new ProductOptionValue
                        {
                            ProductOptionId = parentOption.Id,
                            Name = valName,
                            SortOrder = currentOrder,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                            Status = 1
                        };
                        _context.ProductOptionValues.Add(existingVal);
                        await _context.SaveChangesAsync();
                        parentOption.ProductOptionValues.Add(existingVal);
                    }
                    else
                    {
                        if (!optionSortCounters.ContainsKey(parentOption.Id))
                            optionSortCounters[parentOption.Id] = 0;
                        optionSortCounters[parentOption.Id]++;

                        existingVal.Name = valName;
                        existingVal.SortOrder = optionSortCounters[parentOption.Id];
                        existingVal.Status = 1;
                        existingVal.UpdatedAt = DateTime.Now;
                        existingVal.UpdatedBy = userId;
                    }

                    targetValueIds.Add(existingVal.Id);
                    usedValueIds.Add(existingVal.Id);
                }

                var linksForVariant = allVariantLinks
                    .Where(l => l.ProductVariantId == dbVariant.Id)
                    .ToList();

                foreach (var oldLink in linksForVariant)
                {
                    if (!targetValueIds.Contains(oldLink.ProductOptionValueId))
                    {
                        oldLink.Status = 0;
                        oldLink.UpdatedAt = DateTime.Now;
                        oldLink.UpdatedBy = userId;
                    }
                }

                foreach (var valId in targetValueIds)
                {
                    var existingLink = linksForVariant.FirstOrDefault(l => l.ProductOptionValueId == valId);
                    if (existingLink != null)
                    {
                        existingLink.Status = 1;
                        existingLink.UpdatedAt = DateTime.Now;
                        existingLink.UpdatedBy = userId;
                    }
                    else
                    {
                        var newLink = new ProductVariantOptionValue
                        {
                            ProductVariantId = dbVariant.Id,
                            ProductOptionValueId = valId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                            Status = 1
                        };
                        _context.ProductVariantOptionValues.Add(newLink);
                        allVariantLinks.Add(newLink);
                    }
                }
            }

            foreach (var opt in existingOptions)
            {
                foreach (var val in opt.ProductOptionValues)
                {
                    if (opt.Status == 0 || !usedValueIds.Contains(val.Id))
                    {
                        val.Status = 0;
                        val.UpdatedAt = DateTime.Now;
                        val.UpdatedBy = userId;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
