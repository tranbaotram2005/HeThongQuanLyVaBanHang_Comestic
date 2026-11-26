namespace HeThongBanHang_Comestic_API.DTOs
{
    public class ProductVariantOptionValueResponseDTO
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }

        public int ProductOptionValueId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public string? Notes { get; set; }

        public int? Status { get; set; }
    }
}
