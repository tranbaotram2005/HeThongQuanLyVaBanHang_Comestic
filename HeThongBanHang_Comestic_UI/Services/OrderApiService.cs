using HeThongBanHang_Comestic_UI.DTOs;
using System.Threading.Tasks;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IOrderApiService
    {
        Task<(bool Success, string Message, OrderResponseDTO? Data)> CreateOrderAsync(OrderCreateDTO dto);
        Task<List<OrderResponseDTO>> GetMyOrdersAsync();
        Task<OrderResponseDTO?> GetMyOrderByIdAsync(int orderId);
        Task<(bool Success, string Message)> CancelMyOrderAsync(int orderId);
        Task<List<OrderResponseDTO>> GetAllOrdersAsync();
        Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, int status);
    }

    public class OrderApiService : IOrderApiService
    {
        private readonly HttpClient _httpClient;

        public OrderApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string Message, OrderResponseDTO? Data)> CreateOrderAsync(OrderCreateDTO dto)
        {
            var res = await _httpClient.PostAsJsonAsync("api/Order", dto);
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadFromJsonAsync<MessageWrapper>();
                return (false, err?.Message ?? "Không thể tạo đơn hàng.", null);
            }

            var wrapper = await res.Content.ReadFromJsonAsync<OrderWrapper>();
            return (true, wrapper?.Message ?? "Tạo đơn hàng thành công.", wrapper?.Data);
        }

        public async Task<List<OrderResponseDTO>> GetMyOrdersAsync()
        {
            var res = await _httpClient.GetAsync("api/Order/my");
            if (!res.IsSuccessStatusCode) return new List<OrderResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<OrderResponseDTO>>() ?? new();
        }

        public async Task<OrderResponseDTO?> GetMyOrderByIdAsync(int orderId)
        {
            var res = await _httpClient.GetAsync($"api/Order/my/{orderId}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<OrderResponseDTO>();
        }

        public async Task<(bool Success, string Message)> CancelMyOrderAsync(int orderId)
        {
            var res = await _httpClient.PutAsync($"api/Order/my/{orderId}/cancel", null);
            var body = await res.Content.ReadFromJsonAsync<MessageWrapper>();
            return (res.IsSuccessStatusCode, body?.Message ?? (res.IsSuccessStatusCode ? "Hủy đơn thành công." : "Không thể hủy đơn."));
        }

        private class OrderWrapper
        {
            public string? Message { get; set; }
            public OrderResponseDTO? Data { get; set; }
        }

        private class MessageWrapper
        {
            public string? Message { get; set; }
        }

        public async Task<List<OrderResponseDTO>> GetAllOrdersAsync()
        {
            var res = await _httpClient.GetAsync("api/Order");
            if (!res.IsSuccessStatusCode) return new List<OrderResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<OrderResponseDTO>>() ?? new();
        }
        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, int status)
        {
            var res = await _httpClient.PutAsync($"api/Order/{orderId}/status?status={status}", null);
            var body = await res.Content.ReadFromJsonAsync<MessageWrapper>();
            return (res.IsSuccessStatusCode, body?.Message ?? (res.IsSuccessStatusCode ? "Cập nhật thành công." : "Thất bại."));
        }
    }
}