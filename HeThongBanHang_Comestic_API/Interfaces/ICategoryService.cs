using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface ICategoryService
    {
        // Khách hàng: xem danh sách danh mục
        Task<List<CategoryResponseDTO>> GetAllAsync();

        // Admin, Staff: thêm danh mục
        Task<bool> CreateAsync(CategoryCreateDTO dto, int userId);

        // Admin, Staff: cập nhật danh mục
        Task<bool> UpdateAsync(int id, CategoryUpdateDTO dto, int userId);

        // Admin, Staff: ẩn hoặc hiện danh mục
        Task<bool> UpdateStatusAsync(int id, int status, int userId);
        Task<List<CategoryResponseDTO>> GetAllForAdminAsync();
    }
}
