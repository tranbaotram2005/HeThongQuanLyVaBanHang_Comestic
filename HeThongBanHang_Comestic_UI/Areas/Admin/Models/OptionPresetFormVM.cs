using System.ComponentModel.DataAnnotations;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Models
{
    public class OptionPresetFormVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhóm thuộc tính (VD: Dung tích, Mùi hương).")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập ít nhất 1 giá trị mẫu, phân cách bởi dấu phẩy.")]
        public string ValuesText { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
