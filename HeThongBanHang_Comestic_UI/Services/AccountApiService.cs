using System.Text.Json;
using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Services
{
    public interface IAccountApiService
    {
        Task<(bool Success, string Message, LoginResponseDTO? Data)> LoginAsync(LoginDTO model);
        Task<(bool Success, string Message)> RegisterAsync(RegisterDTO model);
        Task<(bool Success, string Message)> SendRegisterOtpAsync(RegisterDTO model);
        Task<(bool Success, string Message)> ResendRegisterOtpAsync(string email);
        Task<(bool Success, string Message)> VerifyRegisterOtpAsync(VerifyRegisterOtpDTO model);
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
                var err = await ReadMessageSafeAsync(response);
                return (false, err ?? "Tài khoản hoặc mật khẩu không chính xác.", null);
            }

            var result = await response.Content.ReadFromJsonAsync<LoginApiWrapper>();
            if (result?.Data == null)
                return (false, "Không đọc được dữ liệu đăng nhập từ API.", null);

            return (true, result.Message ?? "Đăng nhập thành công!", result.Data);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDTO model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Account/register", model);
            var msg = await ReadMessageSafeAsync(response);
            return (response.IsSuccessStatusCode, msg ?? (response.IsSuccessStatusCode ? "Đăng ký tài khoản thành công!" : "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin."));
        }

        public async Task<(bool Success, string Message)> SendRegisterOtpAsync(RegisterDTO model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Account/register-send-otp", model);
            var msg = await ReadMessageSafeAsync(response);
            return (response.IsSuccessStatusCode, msg ?? (response.IsSuccessStatusCode ? "Đã gửi mã OTP tới email của bạn." : "Không thể gửi mã OTP. Vui lòng kiểm tra lại thông tin."));
        }

        public async Task<(bool Success, string Message)> ResendRegisterOtpAsync(string email)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Account/register-resend-otp", new ResendRegisterOtpDTO { Email = email });
            var msg = await ReadMessageSafeAsync(response);
            return (response.IsSuccessStatusCode, msg ?? (response.IsSuccessStatusCode ? "Đã gửi lại mã OTP." : "Không thể gửi lại mã OTP."));
        }

        public async Task<(bool Success, string Message)> VerifyRegisterOtpAsync(VerifyRegisterOtpDTO model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Account/register-verify-otp", model);
            var msg = await ReadMessageSafeAsync(response);
            return (response.IsSuccessStatusCode, msg ?? (response.IsSuccessStatusCode ? "Xác thực email và đăng ký thành công!" : "Mã OTP không hợp lệ hoặc đã hết hạn."));
        }

        private static async Task<string?> ReadMessageSafeAsync(HttpResponseMessage response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (root.TryGetProperty("message", out var el) || root.TryGetProperty("Message", out el))
                    return el.GetString();

                if (root.TryGetProperty("errors", out var errorsEl) && errorsEl.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in errorsEl.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array && prop.Value.GetArrayLength() > 0)
                            return prop.Value[0].GetString();
                    }
                }
            }
            catch { }
            return raw;
        }

        private class LoginApiWrapper
        {
            public string? Message { get; set; }
            public LoginResponseDTO? Data { get; set; }
        }
    }
}
