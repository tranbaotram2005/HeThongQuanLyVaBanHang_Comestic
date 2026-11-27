namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class CartResponseDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? CouponCode { get; set; }

        public string? Note { get; set; }
    }
}
