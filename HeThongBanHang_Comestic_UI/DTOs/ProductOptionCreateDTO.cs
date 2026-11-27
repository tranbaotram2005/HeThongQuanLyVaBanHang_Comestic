namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class ProductOptionCreateDTO
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = null!;

        public int? SortOrder { get; set; }

        public string? Notes { get; set; }
    }
}
