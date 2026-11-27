using System.ComponentModel.DataAnnotations;

namespace HeThongBanHang_Comestic_API.DTOs
{
    public class VerifyRegisterOtpDTO
    {
        [Required(ErrorMessage = "Thiếu địa chỉ Email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP gồm 6 chữ số.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải gồm đúng 6 chữ số.")]
        public string OtpCode { get; set; } = string.Empty;
    }

    public class ResendRegisterOtpDTO
    {
        [Required(ErrorMessage = "Thiếu địa chỉ Email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;
    }
}
