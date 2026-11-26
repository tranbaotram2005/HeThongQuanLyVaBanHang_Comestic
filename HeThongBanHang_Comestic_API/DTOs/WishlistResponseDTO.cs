namespace HeThongBanHang_Comestic_API.DTOs
{
    public class WishlistResponseDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
