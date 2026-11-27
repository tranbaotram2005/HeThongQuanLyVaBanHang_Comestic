using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Models;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IProductVariantApiService
    {
        Task<List<ProductVariantResponseDTO>> GetByProductAsync(int productId);
        Task<ProductVariantResponseDTO?> GetByIdAsync(int variantId);
        Task<ProductDetailVM?> GetProductDetailAsync(int productId, bool includeHidden = false);
    }

    public class ProductVariantApiService : IProductVariantApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IProductApiService _productApiService;

        public ProductVariantApiService(HttpClient httpClient, IProductApiService productApiService)
        {
            _httpClient = httpClient;
            _productApiService = productApiService;
        }

        public async Task<List<ProductVariantResponseDTO>> GetByProductAsync(int productId)
        {
            var res = await _httpClient.GetAsync($"api/ProductVariant/product/{productId}");
            if (!res.IsSuccessStatusCode) return new List<ProductVariantResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<ProductVariantResponseDTO>>() ?? new();
        }

        public async Task<ProductVariantResponseDTO?> GetByIdAsync(int variantId)
        {
            var res = await _httpClient.GetAsync($"api/ProductVariant/{variantId}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<ProductVariantResponseDTO>();
        }

        public async Task<ProductDetailVM?> GetProductDetailAsync(int productId, bool includeHidden = false)
        {
            var product = includeHidden
                ? (await _productApiService.GetAllForAdminAsync()).FirstOrDefault(p => p.Id == productId)
                : await _productApiService.GetByIdAsync(productId);
            if (product == null) return null;

            // 1. Lấy biến thể và các nhóm thuộc tính của sản phẩm
            var variantsTask = GetByProductAsync(productId);
            var optionsTask = GetOptionsByProductAsync(productId);
            await Task.WhenAll(variantsTask, optionsTask);

            var variants = variantsTask.Result;
            var options = optionsTask.Result;

            // 2. Lấy toàn bộ giá trị thuộc tính (OptionValue) để tra cứu tên
            var optionNameById = options.ToDictionary(o => o.Id, o => o.Name);
            var valueTasks = options.Select(o => GetValuesByOptionAsync(o.Id));
            var allValueLists = await Task.WhenAll(valueTasks);

            var valueMap = allValueLists
                .SelectMany(x => x)
                .ToDictionary(v => v.Id, v => (
                    OptionName: v.ProductOptionId.HasValue && optionNameById.ContainsKey(v.ProductOptionId.Value)
                        ? optionNameById[v.ProductOptionId.Value]
                        : "Phân loại",
                    ValueName: v.Name
                ));

            // 3. Ghép thuộc tính vào từng biến thể
            var variantItems = new List<VariantItemVM>();
            foreach (var v in variants)
            {
                var links = await GetVariantOptionValuesAsync(v.Id);
                var dict = new Dictionary<string, string>();
                foreach (var link in links)
                {
                    if (valueMap.TryGetValue(link.ProductOptionValueId, out var info))
                        dict[info.OptionName] = info.ValueName;
                }

                variantItems.Add(new VariantItemVM
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price ?? product.UnitPrice ?? 0,
                    StockQuantity = v.StockQuantity ?? 0,
                    ImageUrl = v.ImageUrl,
                    IsDefault = v.IsDefault ?? false,
                    Options = dict
                });
            }

            return new ProductDetailVM
            {
                Product = product,
                Variants = variantItems
            };
        }

        private async Task<List<ProductOptionResponseDTO>> GetOptionsByProductAsync(int productId)
        {
            var res = await _httpClient.GetAsync($"api/ProductOption/product/{productId}");
            if (!res.IsSuccessStatusCode) return new();
            return await res.Content.ReadFromJsonAsync<List<ProductOptionResponseDTO>>() ?? new();
        }

        private async Task<List<ProductOptionValueResponseDTO>> GetValuesByOptionAsync(int optionId)
        {
            var res = await _httpClient.GetAsync($"api/ProductOptionValue/option/{optionId}");
            if (!res.IsSuccessStatusCode) return new();
            return await res.Content.ReadFromJsonAsync<List<ProductOptionValueResponseDTO>>() ?? new();
        }

        private async Task<List<ProductVariantOptionValueResponseDTO>> GetVariantOptionValuesAsync(int variantId)
        {
            var res = await _httpClient.GetAsync($"api/ProductVariantOptionValue/variant/{variantId}");
            if (!res.IsSuccessStatusCode) return new();
            return await res.Content.ReadFromJsonAsync<List<ProductVariantOptionValueResponseDTO>>() ?? new();
        }
    }
}