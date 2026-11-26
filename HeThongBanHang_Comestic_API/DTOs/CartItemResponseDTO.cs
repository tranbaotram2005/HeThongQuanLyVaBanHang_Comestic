namespace HeThongBanHang_Comestic_API.DTOs
{
    public class CartItemResponseDTO
    {
        public int Id { get; set; }

        public int CartId { get; set; }

        public int ProductVariantId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? ImageUrl { get; set; }
    }
}
