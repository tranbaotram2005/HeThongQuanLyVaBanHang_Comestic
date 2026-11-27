using System.ComponentModel.DataAnnotations;

namespace HeThongBanHang_Comestic_UI.DTOs
{
    public class EmployeeOrderItemDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sản phẩm.")]
        public int ProductVariantId { get; set; }

        [Range(1, 1000, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; } = 1;
    }

    public class EmployeeOrderCreateDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khách hàng.")]
        public int CustomerId { get; set; }

        public string? Receive { get; set; } = "Mua tại quầy";

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
        public string ShipName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string ShipPhone { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        public string ShipAddress { get; set; } = null!;

        public string? ShipCity { get; set; }
        public string? ShipRegion { get; set; }
        public string? ShipCountry { get; set; } = "Vietnam";
        public string PaymentMethod { get; set; } = "Tiền mặt";
        public string? Notes { get; set; }

        public List<EmployeeOrderItemDTO> Items { get; set; } = new() { new EmployeeOrderItemDTO() };
    }
}
