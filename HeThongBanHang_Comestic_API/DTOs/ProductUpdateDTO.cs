namespace HeThongBanHang_Comestic_API.DTOs
{
    public class ProductUpdateDTO
    {
        public string ProductName { get; set; } = null!;

        public string? Brand { get; set; }

        public string? Description { get; set; }

        public string? QuantityPerUnit { get; set; }

        public int? CategoryId { get; set; }

        public int? SupplierId { get; set; }

        public decimal? UnitPrice { get; set; }

        public short? UnitsInStock { get; set; }

        public short? UnitsOnOrder { get; set; }

        public short? ReorderLevel { get; set; }

        public string? Picture { get; set; }

        public string? Notes { get; set; }
    }
}
