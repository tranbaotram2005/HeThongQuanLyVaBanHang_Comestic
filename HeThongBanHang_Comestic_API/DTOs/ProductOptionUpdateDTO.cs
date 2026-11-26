namespace HeThongBanHang_Comestic_API.DTOs
{
    public class ProductOptionUpdateDTO
    {
        public string Name { get; set; } = null!;

        public int? SortOrder { get; set; }

        public string? Notes { get; set; }
    }
}
