using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IProductVariantOptionValueService
    {
        // Lấy danh sách giá trị thuộc tính theo biến thể
        Task<List<ProductVariantOptionValueResponseDTO>> GetByVariantAsync(int productVariantId);

        // Lấy chi tiết một bản ghi liên kết
        Task<ProductVariantOptionValueResponseDTO?> GetByIdAsync(int id);

        // Thêm giá trị thuộc tính vào biến thể
        Task<bool> CreateAsync(ProductVariantOptionValueCreateDTO dto, int userId);

        // Cập nhật giá trị thuộc tính của biến thể
        Task<bool> UpdateAsync(int id, ProductVariantOptionValueUpdateDTO dto, int userId);

        // Ẩn hoặc kích hoạt lại bản ghi liên kết
        Task<bool> UpdateStatusAsync(int id, int status, int userId);
    }
}
