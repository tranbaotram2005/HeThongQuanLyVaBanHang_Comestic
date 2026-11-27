using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IPaymentApiService
    {
        Task<List<PaymentResponseDTO>> GetMyPaymentsByOrderAsync(int orderId);
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
    }
}