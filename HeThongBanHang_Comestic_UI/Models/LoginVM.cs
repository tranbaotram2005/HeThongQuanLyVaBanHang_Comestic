using System.ComponentModel.DataAnnotations;

namespace HeThongBanHang_Comestic_UI.Models
{
    public class LoginVM
    {

        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string UserName { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
