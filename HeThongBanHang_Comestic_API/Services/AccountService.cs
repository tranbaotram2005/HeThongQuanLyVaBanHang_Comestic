using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.AspNetCore.Identity;
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

        public AccountService(
            WebApiGtEcommerceContext context,
            IConfiguration configuration,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
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
            // 1. Chuẩn hóa dữ liệu
            string userName = model.UserName.Trim();
            string email = model.Email.Trim();

            string normalizedUserName = userName.ToUpperInvariant();
            string normalizedEmail = email.ToUpperInvariant();

            // 2. Kiểm tra Username đã tồn tại chưa
            bool userNameExists = await _context.Users
                .AnyAsync(u => u.NormalizedUserName == normalizedUserName);

            if (userNameExists)
                return false;

            // 3. Kiểm tra Email đã tồn tại chưa
            bool emailExists = await _context.Users
                .AnyAsync(u => u.NormalizedEmail == normalizedEmail);

            if (emailExists)
                return false;

            // 4. Tìm Role Customer
            var customerRole = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.NormalizedName == "CUSTOMER" ||
                    r.Name == "Customer");

            if (customerRole == null)
                return false;

            // 5. Bắt đầu transaction
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 6. Tạo User
                var user = new User
                {
                    FullName = model.FullName.Trim(),
                    UserName = userName,
                    NormalizedUserName = normalizedUserName,
                    Email = email,
                    NormalizedEmail = normalizedEmail,
                    EmailConfirmed = false,
                    PhoneNumber = model.Phone?.Trim(),
                    PhoneNumberConfirmed = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                // 7. Hash mật khẩu
                user.PasswordHash = _passwordHasher.HashPassword(
                    user,
                    model.Password
                );

                // 8. Gán Role Customer
                user.Roles.Add(customerRole);

                _context.Users.Add(user);

                // 9. Tạo hồ sơ Customer và liên kết với User
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

                // 10. Lưu dữ liệu
                await _context.SaveChangesAsync();

                // 11. Xác nhận transaction
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
