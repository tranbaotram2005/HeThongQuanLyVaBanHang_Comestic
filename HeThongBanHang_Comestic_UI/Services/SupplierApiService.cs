using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface ISupplierApiService
    {
        Task<List<SupplierResponseDTO>> GetAllAsync();
        Task<SupplierResponseDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(SupplierCreateDTO dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, SupplierUpdateDTO dto);
        Task<bool> UpdateStatusAsync(int id, int status);
        Task<List<SupplierResponseDTO>> GetAllForAdminAsync();
    }

    public class SupplierApiService : ISupplierApiService
    {
        private readonly HttpClient _httpClient;

        public SupplierApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SupplierResponseDTO>> GetAllAsync()
        {
            var res = await _httpClient.GetAsync("api/Supplier");
            if (!res.IsSuccessStatusCode) return new List<SupplierResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<SupplierResponseDTO>>() ?? new();
        }

        public async Task<SupplierResponseDTO?> GetByIdAsync(int id)
        {
            var res = await _httpClient.GetAsync($"api/Supplier/{id}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<SupplierResponseDTO>();
        }

        public async Task<(bool Success, string Message)> CreateAsync(SupplierCreateDTO dto)
        {
            var res = await _httpClient.PostAsJsonAsync("api/Supplier", dto);
            var msg = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, string.IsNullOrWhiteSpace(msg) ? "Thao tác thất bại." : msg);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, SupplierUpdateDTO dto)
        {
            var res = await _httpClient.PutAsJsonAsync($"api/Supplier/{id}", dto);
            var msg = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, string.IsNullOrWhiteSpace(msg) ? "Thao tác thất bại." : msg);
        }

        public async Task<bool> UpdateStatusAsync(int id, int status)
        {
            var res = await _httpClient.PatchAsJsonAsync($"api/Supplier/{id}/status", new SupplierStatusDTO { Status = status });
            return res.IsSuccessStatusCode;
        }

        public async Task<List<SupplierResponseDTO>> GetAllForAdminAsync()
        {
            var res = await _httpClient.GetAsync("api/Supplier/admin");
            if (!res.IsSuccessStatusCode)
                return await GetAllAsync();

            return await res.Content.ReadFromJsonAsync<List<SupplierResponseDTO>>() ?? new();
        }

    }
}