using System.ComponentModel.DataAnnotations;

namespace HeThongBanHang_Comestic_UI.DTOs
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
        public string Name { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
