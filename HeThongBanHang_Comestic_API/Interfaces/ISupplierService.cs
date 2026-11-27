using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface ISupplierService
    {
        Task<List<SupplierResponseDTO>> GetAllAsync();
        Task<SupplierResponseDTO?> GetByIdAsync(int id);
        Task<bool> CreateAsync(SupplierCreateDTO dto, int userId);
        Task<bool> UpdateAsync(int id, SupplierUpdateDTO dto, int userId);
        Task<bool> UpdateStatusAsync(int id, int status, int userId);
        Task<List<SupplierResponseDTO>> GetAllForAdminAsync();
    }
}