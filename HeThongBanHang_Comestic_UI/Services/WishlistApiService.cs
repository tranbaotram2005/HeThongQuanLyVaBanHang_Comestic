using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IWishlistApiService
    {
        Task<List<WishlistResponseDTO>> GetMyWishlistAsync();
        Task<bool> AddAsync(int productId);
        Task<bool> DeleteAsync(int wishlistId);
    }

    public class WishlistApiService : IWishlistApiService
    {
        private readonly HttpClient _httpClient;

        public WishlistApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WishlistResponseDTO>> GetMyWishlistAsync()
        {
            var res = await _httpClient.GetAsync("api/Wishlist");
            if (!res.IsSuccessStatusCode) return new List<WishlistResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<WishlistResponseDTO>>() ?? new();
        }

        public async Task<bool> AddAsync(int productId)
        {
            var res = await _httpClient.PostAsJsonAsync("api/Wishlist", new WishlistCreateDTO { ProductId = productId });
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int wishlistId)
        {
            var res = await _httpClient.DeleteAsync($"api/Wishlist/{wishlistId}");
            return res.IsSuccessStatusCode;
        }
    }
}