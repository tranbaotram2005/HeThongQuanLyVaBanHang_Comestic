using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IProductApiService
    {
        Task<List<ProductResponseDTO>> GetAllAsync();
        Task<List<ProductResponseDTO>> GetAllForAdminAsync();
        Task<ProductResponseDTO?> GetByIdAsync(int id);
        Task<List<ProductResponseDTO>> GetByCategoryAsync(int categoryId);
        Task<List<ProductResponseDTO>> FilterByPriceAsync(decimal? minPrice, decimal? maxPrice);
        Task<(bool Success, string Message, int? ProductId)> CreateAsync(ProductCreateDTO dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, ProductUpdateDTO dto);
        Task<(bool Success, string Message)> UpdateStatusAsync(int id, int status);
    }

    public class ProductApiService : IProductApiService
    {
        private readonly HttpClient _httpClient;

        public ProductApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductResponseDTO>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/Product");
            if (!response.IsSuccessStatusCode)
                return new List<ProductResponseDTO>();

            return await response.Content.ReadFromJsonAsync<List<ProductResponseDTO>>()
                   ?? new List<ProductResponseDTO>();
        }

        public async Task<List<ProductResponseDTO>> GetAllForAdminAsync()
        {
            var response = await _httpClient.GetAsync("api/Product/admin");
            if (!response.IsSuccessStatusCode)
                return await GetAllAsync();

            return await response.Content.ReadFromJsonAsync<List<ProductResponseDTO>>()
                   ?? new List<ProductResponseDTO>();
        }

        public async Task<ProductResponseDTO?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Product/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ProductResponseDTO>();
        }

        public async Task<List<ProductResponseDTO>> GetByCategoryAsync(int categoryId)
        {
            var response = await _httpClient.GetAsync($"api/Product/category/{categoryId}");
            if (!response.IsSuccessStatusCode)
                return new List<ProductResponseDTO>();

            return await response.Content.ReadFromJsonAsync<List<ProductResponseDTO>>()
                   ?? new List<ProductResponseDTO>();
        }

        public async Task<List<ProductResponseDTO>> FilterByPriceAsync(decimal? minPrice, decimal? maxPrice)
        {
            var queryParams = new List<string>();
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice.Value}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice.Value}");

            var url = "api/Product/filter-price";
            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<ProductResponseDTO>();

            return await response.Content.ReadFromJsonAsync<List<ProductResponseDTO>>()
                   ?? new List<ProductResponseDTO>();
        }

        public async Task<(bool Success, string Message, int? ProductId)> CreateAsync(ProductCreateDTO dto)
        {
            var res = await _httpClient.PostAsJsonAsync("api/Product", dto);
            var raw = await res.Content.ReadAsStringAsync();

            string? message = null;
            int? productId = null;

            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(raw);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("message", out var msgEl) || root.TryGetProperty("Message", out msgEl))
                    {
                        message = msgEl.GetString();
                    }
                    if (root.TryGetProperty("id", out var idEl) && idEl.TryGetInt32(out var parsedId))
                    {
                        productId = parsedId;
                    }
                    else if (root.TryGetProperty("Id", out var idElUpper) && idElUpper.TryGetInt32(out var parsedIdUpper))
                    {
                        productId = parsedIdUpper;
                    }
                }
                catch
                {
                    message = raw;
                }
            }

            return (
                res.IsSuccessStatusCode,
                message ?? (res.IsSuccessStatusCode ? "Thêm thành công." : $"Không thể thêm sản phẩm (HTTP {(int)res.StatusCode})."),
                productId
            );
        }
        public async Task<(bool Success, string Message)> UpdateAsync(int id, ProductUpdateDTO dto)
        {
            var res = await _httpClient.PutAsJsonAsync($"api/Product/{id}", dto);
            var message = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, message ?? (res.IsSuccessStatusCode ? "Cập nhật thành công." : $"Không thể cập nhật sản phẩm (HTTP {(int)res.StatusCode})."));
        }
        public async Task<(bool Success, string Message)> UpdateStatusAsync(int id, int status)
        {
            var res = await _httpClient.PatchAsync($"api/Product/{id}/status?status={status}", null);
            var message = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, message ?? (res.IsSuccessStatusCode ? "Cập nhật trạng thái thành công." : $"Thất bại (HTTP {(int)res.StatusCode})."));
        }

        private static async Task<string?> ReadMessageSafeAsync(HttpResponseMessage res)
        {
            try
            {
                var raw = await res.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw)) return null;
                var body = System.Text.Json.JsonSerializer.Deserialize<MessageWrapper>(
                    raw,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return body?.Message;
            }
            catch
            {
                return null;
            }
        }

        private class MessageWrapper { public string? Message { get; set; } }
    }
}