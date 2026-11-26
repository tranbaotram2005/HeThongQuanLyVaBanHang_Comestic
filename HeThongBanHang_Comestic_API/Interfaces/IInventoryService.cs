using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IInventoryService
    {
        // Lấy danh sách tồn kho
        Task<List<InventoryResponseDTO>> GetAllAsync();

        // Lấy thông tin tồn kho theo ID
        Task<InventoryResponseDTO?> GetByIdAsync(int id);

        // Lấy danh sách tồn kho theo biến thể sản phẩm
        Task<List<InventoryResponseDTO>> GetByVariantAsync(int variantId);

        // Tạo bản ghi tồn kho
        Task<bool> CreateAsync(InventoryCreateDTO dto, int userId);

        // Cập nhật thông tin tồn kho
        Task<bool> UpdateAsync(int id, InventoryUpdateDTO dto, int userId);

        // Cập nhật trạng thái tồn kho
        Task<bool> UpdateStatusAsync(int id, int status, int userId);
    }
}
