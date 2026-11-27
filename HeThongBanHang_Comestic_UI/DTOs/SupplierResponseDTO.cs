namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class SupplierResponseDTO
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? ContactName { get; set; }
        public string? ContactTitle { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? HomePage { get; set; }
        public string? Notes { get; set; }
        public int Status { get; set; }
        public string? Picture { get; set; }
    }
}