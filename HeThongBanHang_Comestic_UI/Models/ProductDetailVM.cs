using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Models
{
    public class ProductDetailVM
    {
        public ProductResponseDTO Product { get; set; } = new();
        public List<VariantItemVM> Variants { get; set; } = new();
    }

    public class VariantItemVM
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsDefault { get; set; }
        // Key = Tên Option (vd: "Dung tích"), Value = Giá trị (vd: "236ml")
        public Dictionary<string, string> Options { get; set; } = new();
    }
}