namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class ProductOptionResponseDTO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string Name { get; set; } = null!;

        public int? SortOrder { get; set; }

        public int? Status { get; set; }
    }
}
