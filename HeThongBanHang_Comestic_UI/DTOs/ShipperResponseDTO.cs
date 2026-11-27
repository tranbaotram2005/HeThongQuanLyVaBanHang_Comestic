namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class ShipperResponseDTO
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Notes { get; set; }
        public int? Status { get; set; }
    }
}
