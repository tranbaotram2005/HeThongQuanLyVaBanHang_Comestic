
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.DTOs.Payment;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IPaymentService
    {
        // Khách hàng xem danh sách thanh toán của đơn hàng mình
        Task<List<PaymentResponseDTO>> GetMyPaymentsAsync(int userId, int orderId);

        // Admin, Employee xem danh sách thanh toán của một đơn hàng
        Task<List<PaymentResponseDTO>> GetPaymentsByOrderIdAsync(int orderId);

        // Admin, Employee xem tất cả thanh toán
        Task<List<PaymentResponseDTO>> GetAllPaymentsAsync();

        // Admin, Employee xem chi tiết một thanh toán
        Task<PaymentResponseDTO?> GetPaymentByIdAsync(int paymentId);

        // Tạo thanh toán
        Task<PaymentResponseDTO?> CreatePaymentAsync(PaymentCreateDTO dto, int userId);

        // Cập nhật trạng thái thanh toán
        Task<bool> UpdatePaymentStatusAsync(
            int paymentId,
            PaymentUpdateStatusDTO dto,
            int userId);

        Task<bool> ProcessSePayWebhookAsync(SePayWebhookDTO dto);
    }
}
