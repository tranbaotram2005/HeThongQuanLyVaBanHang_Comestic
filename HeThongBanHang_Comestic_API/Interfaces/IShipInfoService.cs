
using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IShipInfoService
    {
        // Khách hàng xem thông tin giao hàng của đơn mình
        Task<List<ShipInfoResponseDTO>> GetMyShipInfosAsync(
            int userId, int orderId);

        // Admin, Employee xem thông tin giao hàng theo đơn
        Task<List<ShipInfoResponseDTO>> GetShipInfosByOrderIdAsync(
            int orderId);

        // Admin, Employee xem toàn bộ thông tin giao hàng
        Task<List<ShipInfoResponseDTO>> GetAllShipInfosAsync();

        // Admin, Employee xem chi tiết thông tin giao hàng
        Task<ShipInfoResponseDTO?> GetShipInfoByIdAsync(int shipInfoId);

        // Admin, Employee cập nhật thông tin giao hàng
        Task<bool> UpdateShipInfoAsync(
            int shipInfoId,
            ShipInfoUpdateDTO dto,
            int userId);
    }
}
