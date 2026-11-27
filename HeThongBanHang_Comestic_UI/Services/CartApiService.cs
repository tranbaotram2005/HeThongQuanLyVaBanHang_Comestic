using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface ICartApiService
    {
        Task<List<CartItemResponseDTO>> GetMyCartItemsAsync();
        Task<(bool Success, string Message)> AddItemAsync(int productVariantId, int quantity);
        Task<(bool Success, string Message)> UpdateItemAsync(int cartItemId, int quantity);
        Task<bool> DeleteItemAsync(int cartItemId);
    }

    public class CartApiService : ICartApiService
    {
        private readonly HttpClient _httpClient;

        public CartApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CartItemResponseDTO>> GetMyCartItemsAsync()
        {
            var res = await _httpClient.GetAsync("api/CartItem");
            if (!res.IsSuccessStatusCode) return new List<CartItemResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<CartItemResponseDTO>>() ?? new();
        }

        public async Task<(bool Success, string Message)> AddItemAsync(int productVariantId, int quantity)
        {
            var dto = new CartItemCreateDTO
            {
                ProductVariantId = productVariantId,
                Quantity = quantity
            };
            var res = await _httpClient.PostAsJsonAsync("api/CartItem", dto);
            var message = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, message ?? (res.IsSuccessStatusCode ? "Thành công" : $"Không thể thêm vào giỏ hàng (HTTP {(int)res.StatusCode})."));
        }

        public async Task<(bool Success, string Message)> UpdateItemAsync(int cartItemId, int quantity)
        {
            var dto = new CartItemUpdateDTO { Quantity = quantity };
            var res = await _httpClient.PutAsJsonAsync($"api/CartItem/{cartItemId}", dto);
            var message = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, message ?? (res.IsSuccessStatusCode ? "Thành công" : $"Không thể cập nhật số lượng (HTTP {(int)res.StatusCode})."));
        }

        public async Task<bool> DeleteItemAsync(int cartItemId)
        {
            var res = await _httpClient.DeleteAsync($"api/CartItem/{cartItemId}");
            return res.IsSuccessStatusCode;
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

        private class MessageWrapper
        {
            public string? Message { get; set; }
        }
    }
}