using HeThongBanHang_Comestic_UI.Models;
using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface ICategoryApiService
    {
        Task<List<CategoryResponseDTO>> GetAllAsync();
        Task<bool> CreateAsync(CategoryCreateDTO dto);
        Task<bool> UpdateAsync(int id, CategoryUpdateDTO dto);
        Task<bool> UpdateStatusAsync(int id, int status);
        Task<List<CategoryResponseDTO>> GetAllForAdminAsync();
    }

    public class CategoryApiService : ICategoryApiService
    {
        private readonly HttpClient _httpClient;

        public CategoryApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            // Gửi GET https://localhost:7016/api/Category
            var response = await _httpClient.GetAsync("api/Category");
            if (!response.IsSuccessStatusCode)
                return new List<CategoryResponseDTO>();

            return await response.Content.ReadFromJsonAsync<List<CategoryResponseDTO>>()
                   ?? new List<CategoryResponseDTO>();
        }
        public async Task<bool> CreateAsync(CategoryCreateDTO dto)
    => (await _httpClient.PostAsJsonAsync("api/Category", dto)).IsSuccessStatusCode;
        public async Task<bool> UpdateAsync(int id, CategoryUpdateDTO dto)
            => (await _httpClient.PutAsJsonAsync($"api/Category/{id}", dto)).IsSuccessStatusCode;
        public async Task<bool> UpdateStatusAsync(int id, int status)
            => (await _httpClient.PatchAsJsonAsync($"api/Category/{id}/status", 
                new CategoryStatusDTO { Status = status })).IsSuccessStatusCode;
        public async Task<List<CategoryResponseDTO>> GetAllForAdminAsync()
        {
            var response = await _httpClient.GetAsync("api/Category/admin");
            if (!response.IsSuccessStatusCode)
                return await GetAllAsync();

            return await response.Content.ReadFromJsonAsync<List<CategoryResponseDTO>>()
                   ?? new List<CategoryResponseDTO>();
        }
    }
}