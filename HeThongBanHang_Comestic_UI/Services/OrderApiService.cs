using System.Text.Json;
using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IOrderApiService
    {
        Task<(bool Success, string Message, OrderResponseDTO? Data)> CreateOrderAsync(OrderCreateDTO dto);
        Task<List<OrderResponseDTO>> GetMyOrdersAsync();
        Task<OrderResponseDTO?> GetMyOrderByIdAsync(int orderId);
        Task<(bool Success, string Message)> CancelMyOrderAsync(int orderId);
        Task<List<OrderResponseDTO>> GetAllOrdersAsync();
        Task<(bool Success, string? Error, List<OrderResponseDTO> Orders)> GetAllOrdersWithStatusAsync();
        Task<OrderResponseDTO?> GetOrderByIdAsync(int orderId);
        Task<List<OrderResponseDTO>> GetMyEmployeeOrdersAsync();
        Task<List<CustomerSelectDTO>> GetCustomersAsync();
        Task<(bool Success, string Message, OrderResponseDTO? Data)> CreateOrderByEmployeeAsync(EmployeeOrderCreateDTO dto);
        Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, int status);
        Task<int> GetBadgeCountAsync();
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
                var err = await ReadMessageSafeAsync(res);
                return (false, err ?? $"Không thể tạo đơn hàng (HTTP {(int)res.StatusCode}).", null);
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
            var msg = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, msg ?? (res.IsSuccessStatusCode ? "Hủy đơn thành công." : "Không thể hủy đơn."));
        }

        public async Task<List<OrderResponseDTO>> GetAllOrdersAsync()
        {
            var (_, _, orders) = await GetAllOrdersWithStatusAsync();
            return orders;
        }

        public async Task<(bool Success, string? Error, List<OrderResponseDTO> Orders)> GetAllOrdersWithStatusAsync()
        {
            var res = await _httpClient.GetAsync("api/Order");
            if (!res.IsSuccessStatusCode)
            {
                var msg = await ReadMessageSafeAsync(res);
                return (false, msg ?? $"Không thể tải danh sách đơn hàng (HTTP {(int)res.StatusCode}). Hãy thử đăng xuất rồi đăng nhập lại.", new List<OrderResponseDTO>());
            }

            var data = await res.Content.ReadFromJsonAsync<List<OrderResponseDTO>>() ?? new List<OrderResponseDTO>();
            return (true, null, data);
        }

        public async Task<OrderResponseDTO?> GetOrderByIdAsync(int orderId)
        {
            var res = await _httpClient.GetAsync($"api/Order/{orderId}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<OrderResponseDTO>();
        }

        public async Task<List<OrderResponseDTO>> GetMyEmployeeOrdersAsync()
        {
            var res = await _httpClient.GetAsync("api/Order/my-employee");
            if (!res.IsSuccessStatusCode) return new List<OrderResponseDTO>();
            return await res.Content.ReadFromJsonAsync<List<OrderResponseDTO>>() ?? new();
        }

        public async Task<List<CustomerSelectDTO>> GetCustomersAsync()
        {
            var res = await _httpClient.GetAsync("api/Order/customers");
            if (!res.IsSuccessStatusCode) return new List<CustomerSelectDTO>();
            return await res.Content.ReadFromJsonAsync<List<CustomerSelectDTO>>() ?? new();
        }

        public async Task<(bool Success, string Message, OrderResponseDTO? Data)> CreateOrderByEmployeeAsync(EmployeeOrderCreateDTO dto)
        {
            var res = await _httpClient.PostAsJsonAsync("api/Order/employee-create", dto);
            if (!res.IsSuccessStatusCode)
            {
                var err = await ReadMessageSafeAsync(res);
                return (false, err ?? $"Không thể tạo đơn dùm khách (HTTP {(int)res.StatusCode}).", null);
            }

            var wrapper = await res.Content.ReadFromJsonAsync<OrderWrapper>();
            return (true, wrapper?.Message ?? "Tạo đơn hàng dùm khách thành công.", wrapper?.Data);
        }

        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, int status)
        {
            var res = await _httpClient.PutAsync($"api/Order/{orderId}/status?status={status}", null);
            var msg = await ReadMessageSafeAsync(res);
            return (res.IsSuccessStatusCode, msg ?? (res.IsSuccessStatusCode ? "Cập nhật thành công." : "Thất bại."));
        }

        public async Task<int> GetBadgeCountAsync()
        {
            try
            {
                var res = await _httpClient.GetAsync("api/Order/badge-count");
                if (!res.IsSuccessStatusCode) return 0;
                var body = await res.Content.ReadFromJsonAsync<BadgeWrapper>();
                return body?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private class BadgeWrapper
        {
            public int Count { get; set; }
        }

        private static async Task<string?> ReadMessageSafeAsync(HttpResponseMessage res)
        {
            try
            {
                var raw = await res.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw)) return null;
                var body = JsonSerializer.Deserialize<MessageWrapper>(
                    raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return body?.Message ?? raw.Trim('"');
            }
            catch
            {
                return null;
            }
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
    }
}
