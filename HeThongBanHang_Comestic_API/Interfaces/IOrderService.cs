using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IOrderService
    {
        // Khách hàng tạo đơn hàng từ giỏ hàng
        Task<OrderResponseDTO?> CreateOrderAsync(
            OrderCreateDTO dto,
            int userId);

        // Khách hàng xem danh sách đơn hàng của mình
        Task<List<OrderResponseDTO>> GetMyOrdersAsync(
            int userId);

        // Khách hàng xem chi tiết đơn hàng của mình
        Task<OrderResponseDTO?> GetMyOrderByIdAsync(
            int orderId,
            int userId);

        // Admin / Employee xem tất cả đơn hàng
        Task<List<OrderResponseDTO>> GetAllOrdersAsync();

        // Admin / Employee xem chi tiết một đơn hàng
        Task<OrderResponseDTO?> GetOrderByIdAsync(
            int orderId);

        // Admin / Employee cập nhật trạng thái đơn hàng
        Task<bool> UpdateOrderStatusAsync(
            int orderId,
            int status,
            int userId);

        // Khách hàng hủy đơn hàng
        Task<bool> CancelMyOrderAsync(
            int orderId,
            int userId);

        // Nhân viên xem các đơn hàng do chính nhân viên đó đặt dùm khách
        Task<List<OrderResponseDTO>> GetOrdersByEmployeeAsync(
            int userId);

        // Admin / Employee lấy danh sách khách hàng để đặt đơn dùm
        Task<List<CustomerSelectDTO>> GetCustomersAsync();

        // Admin / Employee đặt đơn hàng dùm khách hàng
        Task<OrderResponseDTO?> CreateOrderByEmployeeAsync(
            EmployeeOrderCreateDTO dto,
            int employeeUserId);

        // Đếm số đơn hàng đang xử lý hiển thị badge trên Header
        Task<int> GetOrderBadgeCountAsync(
            int userId,
            string role);
    }
}