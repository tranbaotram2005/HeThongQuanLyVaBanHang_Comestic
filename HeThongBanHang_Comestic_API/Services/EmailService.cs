using System.Net;
using System.Net.Mail;
using HeThongBanHang_Comestic_API.Interfaces;

namespace HeThongBanHang_Comestic_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendRegisterOtpAsync(string toEmail, string fullName, string otpCode)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var port = _configuration.GetValue<int?>("EmailSettings:Port") ?? 587;
            var senderName = _configuration["EmailSettings:SenderName"] ?? "Comestic Shop";
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var appPassword = _configuration["EmailSettings:AppPassword"];

            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình EmailSettings (SenderEmail / AppPassword) trong appsettings.json của API.");
            }

            string safeName = WebUtility.HtmlEncode(fullName);
            string safeOtp = WebUtility.HtmlEncode(otpCode);

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail.Trim(), senderName),
                Subject = $"[Comestic Shop] Mã xác thực đăng ký: {otpCode}",
                IsBodyHtml = true,
                Body = $@"
                    <div style='font-family:Arial,sans-serif;max-width:520px;margin:0 auto;padding:24px;border:1px solid #e2e8f0;border-radius:10px;'>
                        <h2 style='color:#1e293b;margin-top:0;'>Xác thực tài khoản Comestic Shop</h2>
                        <p>Xin chào <b>{safeName}</b>,</p>
                        <p>Cảm ơn bạn đã đăng ký tài khoản. Vui lòng nhập mã OTP bên dưới để hoàn tất xác thực email:</p>
                        <div style='text-align:center;margin:24px 0;'>
                            <span style='display:inline-block;font-size:28px;font-weight:bold;letter-spacing:6px;padding:12px 24px;background:#fef9c3;color:#854d0e;border:1px dashed #eab308;border-radius:8px;'>
                                {safeOtp}
                            </span>
                        </div>
                        <p style='color:#64748b;font-size:13px;'>Mã OTP có hiệu lực trong <b>5 phút</b>. Vui lòng không chia sẻ mã này cho người khác.</p>
                    </div>"
            };
            message.To.Add(toEmail.Trim());

            using var client = new SmtpClient(smtpServer, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Trim(), appPassword.Replace(" ", string.Empty).Trim()),
                Timeout = 15000
            };

            await client.SendMailAsync(message);
        }
    }
}
