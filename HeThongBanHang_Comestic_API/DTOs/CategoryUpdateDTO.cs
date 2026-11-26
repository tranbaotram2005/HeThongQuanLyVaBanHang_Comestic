namespace HeThongBanHang_Comestic_API.DTOs
{
    public class CategoryUpdateDTO
    {
        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        public string? Picture { get; set; }
    }
}
