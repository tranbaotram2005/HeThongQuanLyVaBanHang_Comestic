using System.ComponentModel.DataAnnotations;

namespace HeThongBanHang_Comestic_API.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập Username hoặc Email")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; } = string.Empty;
    }
}
