using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IPaymentApiService
    {
        Task<List<PaymentResponseDTO>> GetMyPaymentsByOrderAsync(int orderId);
        Task<List<PaymentResponseDTO>> GetAllPaymentsAsync();
        Task<List<PaymentResponseDTO>> GetPaymentsByOrderIdAsync(int orderId);
        Task<PaymentResponseDTO?> GetPaymentByIdAsync(int paymentId);
        Task<(bool Success, string Message)> UpdatePaymentStatusAsync(int paymentId, PaymentUpdateStatusDTO dto);
    }

    public class PaymentApiService : IPaymentApiService
    {
        private readonly HttpClient _httpClient;

        public PaymentApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PaymentResponseDTO>> GetMyPaymentsByOrderAsync(int orderId)
        {
            var res = await _httpClient.GetAsync($"api/Payment/my/order/{orderId}");
            if (!res.IsSuccessStatusCode) return new List<PaymentResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<PaymentResponseDTO>>() ?? new();
        }

        public async Task<List<PaymentResponseDTO>> GetAllPaymentsAsync()
        {
            var res = await _httpClient.GetAsync("api/Payment");
            if (!res.IsSuccessStatusCode) return new List<PaymentResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<PaymentResponseDTO>>() ?? new();
        }

        public async Task<List<PaymentResponseDTO>> GetPaymentsByOrderIdAsync(int orderId)
        {
            var res = await _httpClient.GetAsync($"api/Payment/order/{orderId}");
            if (!res.IsSuccessStatusCode) return new List<PaymentResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<PaymentResponseDTO>>() ?? new();
        }

        public async Task<PaymentResponseDTO?> GetPaymentByIdAsync(int paymentId)
        {
            var res = await _httpClient.GetAsync($"api/Payment/{paymentId}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<PaymentResponseDTO>();
        }

        public async Task<(bool Success, string Message)> UpdatePaymentStatusAsync(int paymentId, PaymentUpdateStatusDTO dto)
        {
            var res = await _httpClient.PutAsJsonAsync($"api/Payment/{paymentId}/status", dto);
            var msg = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, msg ?? (res.IsSuccessStatusCode ? "Cập nhật trạng thái thanh toán thành công." : $"Lỗi HTTP {(int)res.StatusCode}."));
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
