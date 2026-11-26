using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IProductOptionService
    {
        // Lấy danh sách thuộc tính của sản phẩm
        Task<List<ProductOptionResponseDTO>> GetByProductAsync(int productId);

        // Lấy chi tiết thuộc tính
        Task<ProductOptionResponseDTO?> GetByIdAsync(int id);

        // Thêm thuộc tính cho sản phẩm
        Task<bool> CreateAsync(ProductOptionCreateDTO dto, int userId);

        // Cập nhật thuộc tính
        Task<bool> UpdateAsync(int id, ProductOptionUpdateDTO dto, int userId);

        // Ẩn / hiện thuộc tính
        Task<bool> UpdateStatusAsync(int id, int status, int userId);
    }
}
