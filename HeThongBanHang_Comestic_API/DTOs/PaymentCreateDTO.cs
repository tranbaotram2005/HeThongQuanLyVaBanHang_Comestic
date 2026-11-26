
namespace HeThongBanHang_Comestic_API.DTOs
{
    public class PaymentCreateDTO
    {
        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; } = null!;

        public string? Notes { get; set; }
    }
}
