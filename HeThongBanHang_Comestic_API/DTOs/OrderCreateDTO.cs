namespace HeThongBanHang_Comestic_API.DTOs
{
    public class OrderCreateDTO
    {
        // Địa chỉ có sẵn của khách (nếu chọn địa chỉ đã lưu)
        public int? AddressId { get; set; }

        // Thông tin người nhận
        public string ShipName { get; set; } = null!;
        public string ShipPhone { get; set; } = null!;

        // Địa chỉ giao hàng
        public string ShipAddress { get; set; } = null!;
        public int? CountryId { get; set; }
        public int? ProvinceId { get; set; }
        public int? WardId { get; set; }
        public int? TownId { get; set; }

        public string? ShipCity { get; set; }
        public string? ShipRegion { get; set; }
        public string? ShipPostalCode { get; set; }
        public string? ShipCountry { get; set; }

        // Phương thức thanh toán: COD hoặc QR
        public string PaymentMethod { get; set; } = null!;

        // Ghi chú đơn hàng
        public string? Notes { get; set; }
    }
}