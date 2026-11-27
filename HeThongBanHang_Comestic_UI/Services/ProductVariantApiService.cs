using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Models;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IProductVariantApiService
    {
        Task<List<ProductVariantResponseDTO>> GetByProductAsync(int productId);
        Task<List<ProductVariantResponseDTO>> GetByProductForAdminAsync(int productId);
        Task<ProductVariantResponseDTO?> GetByIdAsync(int variantId);
        Task<ProductDetailVM?> GetProductDetailAsync(int productId, bool includeHidden = false);
        Task<(bool Success, string Message)> CreateAsync(ProductVariantCreateDTO dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, ProductVariantUpdateDTO dto);
        Task<(bool Success, string Message)> UpdateStatusAsync(int id, int status);
        Task<(bool Success, string Message)> SyncOptionsAsync(int productId, ProductOptionsSyncDTO dto);
        Task<(List<string> OptionNames, Dictionary<int, Dictionary<string, string>> VariantOptions)> GetAdminOptionsByProductAsync(int productId, IEnumerable<int> variantIds);
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

        public async Task<List<ProductVariantResponseDTO>> GetByProductForAdminAsync(int productId)
        {
            var res = await _httpClient.GetAsync($"api/ProductVariant/product/{productId}/admin");
            if (!res.IsSuccessStatusCode) return new List<ProductVariantResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<ProductVariantResponseDTO>>() ?? new();
        }

        public async Task<(bool Success, string Message)> CreateAsync(ProductVariantCreateDTO dto)
        {
            var res = await _httpClient.PostAsJsonAsync("api/ProductVariant", dto);
            var message = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, message ?? (res.IsSuccessStatusCode ? "Thêm biến thể thành công." : $"Không thể thêm biến thể (HTTP {(int)res.StatusCode})."));
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, ProductVariantUpdateDTO dto)
        {
            var res = await _httpClient.PutAsJsonAsync($"api/ProductVariant/{id}", dto);
            var message = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, message ?? (res.IsSuccessStatusCode ? "Cập nhật biến thể thành công." : $"Không thể cập nhật biến thể (HTTP {(int)res.StatusCode})."));
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(int id, int status)
        {
            var res = await _httpClient.PatchAsync($"api/ProductVariant/{id}/status/{status}", null);
            var message = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, message ?? (res.IsSuccessStatusCode ? "Cập nhật trạng thái biến thể thành công." : $"Không thể cập nhật trạng thái biến thể (HTTP {(int)res.StatusCode})."));
        }

        public async Task<(bool Success, string Message)> SyncOptionsAsync(int productId, ProductOptionsSyncDTO dto)
        {
            var res = await _httpClient.PutAsJsonAsync($"api/ProductVariant/product/{productId}/options", dto);
            var message = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, message ?? (res.IsSuccessStatusCode ? "Đồng bộ thuộc tính sản phẩm thành công." : $"Không thể đồng bộ thuộc tính (HTTP {(int)res.StatusCode})."));
        }

        public async Task<(List<string> OptionNames, Dictionary<int, Dictionary<string, string>> VariantOptions)> GetAdminOptionsByProductAsync(int productId, IEnumerable<int> variantIds)
        {
            var options = await GetOptionsByProductAsync(productId);
            var optionNames = options.Select(o => o.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

            var variantOptions = new Dictionary<int, Dictionary<string, string>>();
            if (!options.Any())
                return (optionNames, variantOptions);

            var optionNameById = options.ToDictionary(o => o.Id, o => o.Name);
            var allValueLists = await Task.WhenAll(options.Select(o => GetValuesByOptionAsync(o.Id)));
            var valueMap = allValueLists
                .SelectMany(x => x)
                .ToDictionary(v => v.Id, v => (
                    OptionName: v.ProductOptionId.HasValue && optionNameById.TryGetValue(v.ProductOptionId.Value, out var optName)
                        ? optName
                        : "Phân loại",
                    ValueName: v.Name
                ));

            var idList = variantIds.Distinct().ToList();
            var linkTasks = idList.Select(async vid => (VariantId: vid, Links: await GetVariantOptionValuesAsync(vid)));
            var allLinks = await Task.WhenAll(linkTasks);

            foreach (var item in allLinks)
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var link in item.Links)
                {
                    if (valueMap.TryGetValue(link.ProductOptionValueId, out var info))
                        dict[info.OptionName] = info.ValueName;
                }
                variantOptions[item.VariantId] = dict;
            }

            return (optionNames, variantOptions);
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

        private static async Task<string?> ReadMessageSafeAsync(HttpResponseMessage res)
        {
            var raw = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (root.TryGetProperty("message", out var msgEl) || root.TryGetProperty("Message", out msgEl))
                    return msgEl.GetString();
            }
            catch { }
            return raw;
        }
    }
}