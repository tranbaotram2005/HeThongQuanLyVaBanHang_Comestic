using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IAccountApiService
    {
        Task<(bool Success, string Message, LoginResponseDTO? Data)> LoginAsync(LoginDTO model);
        Task<(bool Success, string Message)> RegisterAsync(RegisterDTO model);
    }

    public class AccountApiService : IAccountApiService
    {
        private readonly HttpClient _httpClient;

        public AccountApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string Message, LoginResponseDTO? Data)> LoginAsync(LoginDTO model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Account/login", model);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiMessageWrapper>();
                return (false, err?.Message ?? "Tài khoản hoặc mật khẩu không chính xác.", null);
            }

            var result = await response.Content.ReadFromJsonAsync<LoginApiWrapper>();
            if (result?.Data == null)
                return (false, "Không đọc được dữ liệu đăng nhập từ API.", null);

            return (true, result.Message ?? "Đăng nhập thành công!", result.Data);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDTO model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Account/register", model);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiMessageWrapper>();
                return (false, err?.Message ?? "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.");
            }

            var ok = await response.Content.ReadFromJsonAsync<ApiMessageWrapper>();
            return (true, ok?.Message ?? "Đăng ký tài khoản thành công!");
        }

        // Class phụ để hứng đúng cấu trúc { message, data } mà API trả về
        private class LoginApiWrapper
        {
            public string? Message { get; set; }
            public LoginResponseDTO? Data { get; set; }
        }

        private class ApiMessageWrapper
        {
            public string? Message { get; set; }
        }
    }
}