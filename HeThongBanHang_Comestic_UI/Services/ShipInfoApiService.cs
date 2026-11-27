using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IShipInfoApiService
    {
        Task<List<ShipInfoResponseDTO>> GetMyShipInfosByOrderAsync(int orderId);
        Task<List<ShipInfoResponseDTO>> GetAllShipInfosAsync();
        Task<List<ShipInfoResponseDTO>> GetShipInfosByOrderIdAsync(int orderId);
        Task<ShipInfoResponseDTO?> GetShipInfoByIdAsync(int shipInfoId);
        Task<(bool Success, string Message)> UpdateShipInfoAsync(int shipInfoId, ShipInfoUpdateDTO dto);
        Task<List<ShipperResponseDTO>> GetShippersAsync();
    }

    public class ShipInfoApiService : IShipInfoApiService
    {
        private readonly HttpClient _httpClient;

        public ShipInfoApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ShipInfoResponseDTO>> GetMyShipInfosByOrderAsync(int orderId)
        {
            var res = await _httpClient.GetAsync($"api/ShipInfo/my/order/{orderId}");
            if (!res.IsSuccessStatusCode) return new List<ShipInfoResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<ShipInfoResponseDTO>>() ?? new();
        }

        public async Task<List<ShipInfoResponseDTO>> GetAllShipInfosAsync()
        {
            var res = await _httpClient.GetAsync("api/ShipInfo");
            if (!res.IsSuccessStatusCode) return new List<ShipInfoResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<ShipInfoResponseDTO>>() ?? new();
        }

        public async Task<List<ShipInfoResponseDTO>> GetShipInfosByOrderIdAsync(int orderId)
        {
            var res = await _httpClient.GetAsync($"api/ShipInfo/order/{orderId}");
            if (!res.IsSuccessStatusCode) return new List<ShipInfoResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<ShipInfoResponseDTO>>() ?? new();
        }

        public async Task<ShipInfoResponseDTO?> GetShipInfoByIdAsync(int shipInfoId)
        {
            var res = await _httpClient.GetAsync($"api/ShipInfo/{shipInfoId}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<ShipInfoResponseDTO>();
        }

        public async Task<(bool Success, string Message)> UpdateShipInfoAsync(int shipInfoId, ShipInfoUpdateDTO dto)
        {
            var res = await _httpClient.PutAsJsonAsync($"api/ShipInfo/{shipInfoId}", dto);
            var msg = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, msg ?? (res.IsSuccessStatusCode ? "Cập nhật thông tin giao hàng thành công." : $"Lỗi HTTP {(int)res.StatusCode}."));
        }

        public async Task<List<ShipperResponseDTO>> GetShippersAsync()
        {
            var res = await _httpClient.GetAsync("api/Shipper");
            if (!res.IsSuccessStatusCode) return new List<ShipperResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<ShipperResponseDTO>>() ?? new();
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
