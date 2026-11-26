namespace HeThongBanHang_Comestic_API.DTOs
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
