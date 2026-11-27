namespace HeThongBanHang_Comestic_UI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductVariantId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string VariantText { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => Quantity * Price;
    }
}
