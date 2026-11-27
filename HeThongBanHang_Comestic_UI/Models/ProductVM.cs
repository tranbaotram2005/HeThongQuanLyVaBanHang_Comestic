namespace HeThongBanHang_Comestic_UI.Models
{
    public class ProductVM
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public string productImage { get; set; }
        public decimal productPrice { get; set; }
        public int Status { get; set; } = 1;
    }
}
