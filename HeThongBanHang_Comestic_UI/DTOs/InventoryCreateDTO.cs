
namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class InventoryCreateDTO
    {
        public int VariantId { get; set; }

        public int? Quantity { get; set; }

        public int? WarehouseId { get; set; }

        public string? Notes { get; set; }
    }
}
