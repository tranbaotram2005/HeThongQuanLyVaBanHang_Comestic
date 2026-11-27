namespace HeThongBanHang_Comestic_UI.Models
{
    public class WishlistItemVM
    {
        public int Id { get; set; } // Wishlist Id dùng để xóa
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }
}