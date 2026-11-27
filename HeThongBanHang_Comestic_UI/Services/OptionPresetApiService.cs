using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IOptionPresetApiService
    {
        Task<List<OptionPresetDTO>> GetAllAsync();
        Task<List<OptionPresetDTO>> GetActiveAsync();
        Task<OptionPresetDTO?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(OptionPresetUpsertDTO dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, OptionPresetUpsertDTO dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }

    public class OptionPresetApiService : IOptionPresetApiService
    {
        private readonly HttpClient _httpClient;

        public OptionPresetApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<OptionPresetDTO>> GetAllAsync()
        {
            var res = await _httpClient.GetAsync("api/OptionPreset");
            if (!res.IsSuccessStatusCode) return new List<OptionPresetDTO>();
            return await res.Content.ReadFromJsonAsync<List<OptionPresetDTO>>() ?? new();
        }

        public async Task<List<OptionPresetDTO>> GetActiveAsync()
        {
            var res = await _httpClient.GetAsync("api/OptionPreset/active");
            if (!res.IsSuccessStatusCode) return new List<OptionPresetDTO>();
            return await res.Content.ReadFromJsonAsync<List<OptionPresetDTO>>() ?? new();
        }

        public async Task<OptionPresetDTO?> GetByIdAsync(int id)
        {
            var res = await _httpClient.GetAsync($"api/OptionPreset/{id}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<OptionPresetDTO>();
        }

        public async Task<(bool Success, string Message)> CreateAsync(OptionPresetUpsertDTO dto)
        {
            var res = await _httpClient.PostAsJsonAsync("api/OptionPreset", dto);
            var msg = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, msg ?? (res.IsSuccessStatusCode ? "Thêm Option mẫu thành công." : $"Lỗi HTTP {(int)res.StatusCode}."));
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, OptionPresetUpsertDTO dto)
        {
            var res = await _httpClient.PutAsJsonAsync($"api/OptionPreset/{id}", dto);
            var msg = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, msg ?? (res.IsSuccessStatusCode ? "Cập nhật Option mẫu thành công." : $"Lỗi HTTP {(int)res.StatusCode}."));
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var res = await _httpClient.DeleteAsync($"api/OptionPreset/{id}");
            var msg = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, msg ?? (res.IsSuccessStatusCode ? "Đã xóa Option mẫu." : $"Lỗi HTTP {(int)res.StatusCode}."));
        }

        private static async Task<string?> ReadMessageSafeAsync(HttpResponseMessage res)
        {
            var raw = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var el) || doc.RootElement.TryGetProperty("Message", out el))
                    return el.GetString();
            }
            catch { }
            return raw;
        }
    }
}
