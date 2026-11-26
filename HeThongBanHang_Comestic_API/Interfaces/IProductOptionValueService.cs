using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IProductOptionValueService
    {
        // Lấy danh sách giá trị theo thuộc tính
        Task<List<ProductOptionValueResponseDTO>> GetByOptionAsync(int productOptionId);

        // Lấy chi tiết giá trị
        Task<ProductOptionValueResponseDTO?> GetByIdAsync(int id);

        // Thêm giá trị thuộc tính
        Task<bool> CreateAsync(ProductOptionValueCreateDTO dto, int userId);

        // Cập nhật giá trị thuộc tính
        Task<bool> UpdateAsync(int id, ProductOptionValueUpdateDTO dto, int userId);

        // Ẩn / hiện giá trị thuộc tính
        Task<bool> UpdateStatusAsync(int id, int status, int userId);
    }
}
