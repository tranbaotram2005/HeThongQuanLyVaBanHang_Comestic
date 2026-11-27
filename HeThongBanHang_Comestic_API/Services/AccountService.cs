using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using HeThongBanHang_Comestic_API.Interfaces;

namespace HeThongBanHang_Comestic_API.Services
{
    public class AccountService : IAccountService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;

        public AccountService(
            WebApiGtEcommerceContext context,
            IConfiguration configuration,
            IPasswordHasher<User> passwordHasher,
            IMemoryCache cache,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginDTO model)
        {
            // 1. Chuẩn hóa Username hoặc Email người dùng nhập
            // 1. Chuẩn hóa Username hoặc Email về chữ thường
            string login = model.UserNameOrEmail.Trim().ToLower();

            // 2. Tìm trực tiếp theo UserName hoặc Email (không cần Normalized in hoa)
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u =>
                    (u.UserName != null && u.UserName.ToLower() == login) ||
                    (u.Email != null && u.Email.ToLower() == login));

            // 3. Kiểm tra tài khoản tồn tại
            if (user == null)
                return null;

            // 4. Kiểm tra tài khoản có hoạt động không
            if (!user.IsActive)
                return null;

            // 5. Kiểm tra mật khẩu
            if (string.IsNullOrEmpty(user.PasswordHash))
                return null;

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                model.Password
            );

            if (passwordResult == PasswordVerificationResult.Failed)
                return null;

            // 6. Lấy danh sách quyền của người dùng
            var roles = user.Roles
                .Where(r => !string.IsNullOrEmpty(r.Name))
                .Select(r => r.Name!)
                .ToList();

            // 7. Đọc cấu hình JWT
            var jwtKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("Chưa cấu hình Jwt:Key.");

            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException(
                    "Chưa cấu hình Jwt:Issuer hoặc Jwt:Audience."
                );

            var expireMinutes = _configuration.GetValue<int>(
                "Jwt:ExpireMinutes"
            );

            if (expireMinutes <= 0)
                expireMinutes = 60;

            // 8. Tạo các Claims chứa thông tin người dùng
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.UserName ?? string.Empty
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? string.Empty
                ),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()
                )
            };

            // Thêm từng Role vào Token
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 9. Tạo JWT
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var expires = DateTime.UtcNow.AddMinutes(expireMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            string accessToken = new JwtSecurityTokenHandler()
                .WriteToken(token);

            // 10. Trả kết quả đăng nhập
            return new LoginResponseDTO
            {
                AccessToken = accessToken,
                ExpiresAt = expires,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles
            };
        }


        public async Task<bool> RegisterAsync(RegisterDTO model)
        {
            var (success, _) = await CreateUserAndCustomerAsync(model, emailConfirmed: false);
            return success;
        }

        public async Task<(bool Success, string Message)> SendRegisterOtpAsync(RegisterDTO model)
        {
            string userName = model.UserName.Trim();
            string email = model.Email.Trim();
            string normalizedUserName = userName.ToUpperInvariant();
            string normalizedEmail = email.ToUpperInvariant();

            bool userNameExists = await _context.Users.AnyAsync(u =>
                u.NormalizedUserName == normalizedUserName ||
                (u.UserName != null && u.UserName.ToUpper() == normalizedUserName));

            if (userNameExists)
                return (false, "Tên đăng nhập (Username) đã tồn tại.");

            bool emailExists = await _context.Users.AnyAsync(u =>
                u.NormalizedEmail == normalizedEmail ||
                (u.Email != null && u.Email.ToUpper() == normalizedEmail));

            if (emailExists)
                return (false, "Email này đã được đăng ký tài khoản.");

            var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var cacheKey = GetOtpCacheKey(normalizedEmail);
            var pending = new PendingRegistration
            {
                Data = new RegisterDTO
                {
                    FullName = model.FullName.Trim(),
                    UserName = userName,
                    Email = email,
                    Phone = model.Phone?.Trim(),
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword
                },
                OtpCode = otpCode,
                SentAtUtc = DateTime.UtcNow,
                Attempts = 0
            };

            _cache.Set(cacheKey, pending, TimeSpan.FromMinutes(5));

            try
            {
                await _emailService.SendRegisterOtpAsync(email, pending.Data.FullName, otpCode);
                return (true, $"Đã gửi mã OTP gồm 6 chữ số tới {email}. Mã có hiệu lực trong 5 phút.");
            }
            catch (Exception ex)
            {
                _cache.Remove(cacheKey);
                return (false, $"Không gửi được email xác thực: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ResendRegisterOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Thiếu địa chỉ Email.");

            string normalizedEmail = email.Trim().ToUpperInvariant();
            string cacheKey = GetOtpCacheKey(normalizedEmail);

            if (!_cache.TryGetValue<PendingRegistration>(cacheKey, out var pending) || pending == null)
                return (false, "Phiên đăng ký đã hết hạn, vui lòng điền lại thông tin đăng ký.");

            var secondsSinceLast = (DateTime.UtcNow - pending.SentAtUtc).TotalSeconds;
            if (secondsSinceLast < 30)
            {
                int waitSec = Math.Max(1, (int)Math.Ceiling(30 - secondsSinceLast));
                return (false, $"Vui lòng đợi {waitSec} giây trước khi gửi lại mã OTP.");
            }

            var newOtp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            pending.OtpCode = newOtp;
            pending.SentAtUtc = DateTime.UtcNow;
            pending.Attempts = 0;

            _cache.Set(cacheKey, pending, TimeSpan.FromMinutes(5));

            try
            {
                await _emailService.SendRegisterOtpAsync(pending.Data.Email, pending.Data.FullName, newOtp);
                return (true, $"Đã gửi lại mã OTP mới tới {pending.Data.Email}.");
            }
            catch (Exception ex)
            {
                return (false, $"Không gửi được email xác thực: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> VerifyRegisterOtpAsync(VerifyRegisterOtpDTO model)
        {
            string normalizedEmail = model.Email.Trim().ToUpperInvariant();
            string cacheKey = GetOtpCacheKey(normalizedEmail);

            if (!_cache.TryGetValue<PendingRegistration>(cacheKey, out var pending) || pending == null)
                return (false, "Mã OTP đã hết hạn hoặc không tồn tại. Vui lòng đăng ký lại.");

            if (pending.Attempts >= 5)
            {
                _cache.Remove(cacheKey);
                return (false, "Bạn đã nhập sai OTP quá 5 lần. Vui lòng đăng ký lại.");
            }

            if (!string.Equals(pending.OtpCode, model.OtpCode.Trim(), StringComparison.Ordinal))
            {
                pending.Attempts++;
                _cache.Set(cacheKey, pending, TimeSpan.FromMinutes(5));
                return (false, $"Mã OTP không chính xác (còn {5 - pending.Attempts} lần thử).");
            }

            var (created, message) = await CreateUserAndCustomerAsync(pending.Data, emailConfirmed: true);
            if (!created)
                return (false, message);

            _cache.Remove(cacheKey);
            return (true, "Xác thực email và đăng ký tài khoản thành công!");
        }

        private async Task<(bool Success, string Message)> CreateUserAndCustomerAsync(RegisterDTO model, bool emailConfirmed)
        {
            string userName = model.UserName.Trim();
            string email = model.Email.Trim();

            string normalizedUserName = userName.ToUpperInvariant();
            string normalizedEmail = email.ToUpperInvariant();

            bool userNameExists = await _context.Users.AnyAsync(u =>
                u.NormalizedUserName == normalizedUserName ||
                (u.UserName != null && u.UserName.ToUpper() == normalizedUserName));

            if (userNameExists)
                return (false, "Username đã tồn tại.");

            bool emailExists = await _context.Users.AnyAsync(u =>
                u.NormalizedEmail == normalizedEmail ||
                (u.Email != null && u.Email.ToUpper() == normalizedEmail));

            if (emailExists)
                return (false, "Email đã tồn tại.");

            var customerRole = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.NormalizedName == "CUSTOMER" ||
                    r.Name == "Customer");

            if (customerRole == null)
                return (false, "Role Customer chưa được cấu hình.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    FullName = model.FullName.Trim(),
                    UserName = userName,
                    NormalizedUserName = normalizedUserName,
                    Email = email,
                    NormalizedEmail = normalizedEmail,
                    EmailConfirmed = emailConfirmed,
                    PhoneNumber = model.Phone?.Trim(),
                    PhoneNumberConfirmed = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
                user.Roles.Add(customerRole);

                _context.Users.Add(user);

                var customer = new Customer
                {
                    User = user,
                    ContactName = model.FullName.Trim(),
                    Email = email,
                    Phone = model.Phone?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    Status = 1
                };

                _context.Set<Customer>().Add(customer);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Đăng ký tài khoản thành công!");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static string GetOtpCacheKey(string normalizedEmail) => $"pending_register_otp:{normalizedEmail}";

        private sealed class PendingRegistration
        {
            public RegisterDTO Data { get; set; } = new();
            public string OtpCode { get; set; } = string.Empty;
            public DateTime SentAtUtc { get; set; }
            public int Attempts { get; set; }
        }
    }
}
