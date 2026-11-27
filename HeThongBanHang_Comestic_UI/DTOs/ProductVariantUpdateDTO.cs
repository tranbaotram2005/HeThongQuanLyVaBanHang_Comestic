namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class ProductVariantUpdateDTO
    {
        public string? Sku { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public decimal? Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public int? StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsDefault { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public string? Notes { get; set; }
    }
}
