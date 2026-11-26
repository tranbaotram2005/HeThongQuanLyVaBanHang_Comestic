using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface ICartItemService
    {
        // Lấy danh sách sản phẩm trong giỏ hàng
        Task<List<CartItemResponseDTO>> GetMyCartItemsAsync(int userId);

        // Thêm sản phẩm vào giỏ hàng
        Task<bool> CreateAsync(CartItemCreateDTO dto, int userId);

        // Cập nhật số lượng sản phẩm trong giỏ
        Task<bool> UpdateAsync(int id, CartItemUpdateDTO dto, int userId);

        // Xóa sản phẩm khỏi giỏ hàng
        Task<bool> DeleteAsync(int id, int userId);
    }
}
