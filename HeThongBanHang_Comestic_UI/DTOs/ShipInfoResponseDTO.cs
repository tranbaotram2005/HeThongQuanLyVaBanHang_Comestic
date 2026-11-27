namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class ShipInfoResponseDTO
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ShipperId { get; set; }

        public int? CountryId { get; set; }

        public int? ProvinceId { get; set; }

        public int? WardId { get; set; }

        public int? TownId { get; set; }

        public int? AddressId { get; set; }

        public DateTime? RequiredDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public string? ShipAddress { get; set; }

        public string? ShipName { get; set; }

        public string? ShipPhone { get; set; }

        public string? ShipCity { get; set; }

        public string? ShipRegion { get; set; }

        public string? ShipPostalCode { get; set; }

        public string? ShipCountry { get; set; }

        public string? Notes { get; set; }

        public int? Status { get; set; }
    }
}