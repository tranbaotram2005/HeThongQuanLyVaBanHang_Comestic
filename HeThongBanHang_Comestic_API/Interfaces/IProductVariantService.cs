using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IProductVariantService
    {
        // Lấy danh sách biến thể theo sản phẩm
        Task<List<ProductVariantResponseDTO>> GetByProductAsync(int productId);

        // Lấy chi tiết một biến thể
        Task<ProductVariantResponseDTO?> GetByIdAsync(int id);

        // Thêm biến thể mới
        Task<bool> CreateAsync(ProductVariantCreateDTO dto, int userId);

        // Cập nhật biến thể
        Task<bool> UpdateAsync(int id, ProductVariantUpdateDTO dto, int userId);

        // Ẩn hoặc kích hoạt lại biến thể
        Task<bool> UpdateStatusAsync(int id, int status, int userId);
    }
}
