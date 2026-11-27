using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IProductService
    {
        // Lấy danh sách sản phẩm
        Task<List<ProductResponseDTO>> GetAllAsync();

        // Lấy chi tiết sản phẩm theo ID
        Task<ProductResponseDTO?> GetByIdAsync(int id);

        // Lấy sản phẩm theo danh mục
        Task<List<ProductResponseDTO>> GetByCategoryAsync(int categoryId);

        // Thêm sản phẩm
        Task<bool> CreateAsync(ProductCreateDTO dto, int userId);
        Task<int?> CreateAndReturnIdAsync(ProductCreateDTO dto, int userId);

        // Cập nhật sản phẩm
        Task<bool> UpdateAsync(int id, ProductUpdateDTO dto, int userId);

        // Ẩn / hiện sản phẩm
        Task<bool> UpdateStatusAsync(int id, int status, int userId);

        // Lọc sản phẩm theo khoảng giá
        Task<List<ProductResponseDTO>> FilterByPriceAsync(decimal? minPrice, decimal? maxPrice);

        // Admin, Employee: lấy toàn bộ sản phẩm (kể cả đã ẩn)
        Task<List<ProductResponseDTO>> GetAllForAdminAsync();
    }
}
