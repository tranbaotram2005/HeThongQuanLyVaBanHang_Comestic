namespace HeThongBanHang_Comestic_API.DTOs
{
    public class OrderResponseDTO
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }

        public int? EmployeeId { get; set; }

        public DateTime? OrderDate { get; set; }

        public decimal? Freight { get; set; }

        public string? Receive { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public string? Notes { get; set; }

        public int Status { get; set; }

        // Tổng tiền đơn hàng
        public decimal TotalAmount { get; set; }

        // Thông tin chuyển khoản SePay
        public string? TransferContent { get; set; }

        public string? BankAccount { get; set; }

        public string? BankName { get; set; }

        public string? ApprovedByName { get; set; }

        // Chi tiết sản phẩm
        public List<OrderDetailResponseDTO> OrderDetails { get; set; }
            = new List<OrderDetailResponseDTO>();

        // Thông tin thanh toán
        public List<PaymentResponseDTO> Payments { get; set; }
            = new List<PaymentResponseDTO>();

        // Thông tin giao hàng
        public List<ShipInfoResponseDTO> ShipInfos { get; set; }
            = new List<ShipInfoResponseDTO>();
    }
}