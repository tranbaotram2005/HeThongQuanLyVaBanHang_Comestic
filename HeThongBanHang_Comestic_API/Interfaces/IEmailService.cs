namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IEmailService
    {
        Task SendRegisterOtpAsync(string toEmail, string fullName, string otpCode);
    }
}
