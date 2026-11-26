using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface IWishlistService
    {
        // Lấy danh sách sản phẩm yêu thích của người dùng
        Task<List<WishlistResponseDTO>> GetMyWishlistAsync(int userId);

        // Thêm sản phẩm vào danh sách yêu thích
        Task<bool> CreateAsync(WishlistCreateDTO dto, int userId);

        // Xóa sản phẩm khỏi danh sách yêu thích
        Task<bool> DeleteAsync(int id, int userId);
    }
}
