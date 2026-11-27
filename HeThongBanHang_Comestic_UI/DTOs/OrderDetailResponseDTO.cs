namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class OrderDetailResponseDTO
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        public decimal UnitPrice { get; set; }

        public short Quantity { get; set; }

        public string? ProductName { get; set; }

        public string? Sku { get; set; }

        public string? VariantInfo { get; set; }

        public decimal? LineTotal { get; set; }

        public float? Discount { get; set; }

        public string? Notes { get; set; }

        public int? Status { get; set; }
    }
}