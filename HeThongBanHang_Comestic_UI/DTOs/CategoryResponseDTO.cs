namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class CategoryResponseDTO
    {
        public int Id { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        public string? Picture { get; set; }

        public int ProductCount { get; set; }
        public int Status { get; set; }
    }
}
