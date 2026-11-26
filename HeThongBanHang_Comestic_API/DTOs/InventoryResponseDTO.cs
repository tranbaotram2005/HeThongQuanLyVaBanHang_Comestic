
namespace HeThongBanHang_Comestic_API.DTOs
{
    public class InventoryResponseDTO
    {
        public int Id { get; set; }

        public int VariantId { get; set; }

        public int? Quantity { get; set; }

        public int? WarehouseId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public string? Notes { get; set; }

        public int? Status { get; set; }
    }
}
