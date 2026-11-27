namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class PaymentResponseDTO
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string? Method { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? Notes { get; set; }

        public int? Status { get; set; }
    }
}