using System.ComponentModel.DataAnnotations;

namespace HeThongBanHang_Comestic_API.DTOs
{
    public class OptionPresetDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class OptionPresetUpsertDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên nhóm thuộc tính.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public List<string> Values { get; set; } = new();

        [StringLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
