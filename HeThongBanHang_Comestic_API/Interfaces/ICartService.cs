using HeThongBanHang_Comestic_API.DTOs;

namespace HeThongBanHang_Comestic_API.Interfaces
{
    public interface ICartService
    {
        // Lấy giỏ hàng của người dùng đang đăng nhập
        Task<CartResponseDTO?> GetMyCartAsync(int userId);

        // Tạo giỏ hàng cho người dùng
        Task<bool> CreateAsync(CartCreateDTO dto, int userId);

        // Cập nhật mã giảm giá và ghi chú
        Task<bool> UpdateAsync(CartUpdateDTO dto, int userId);

        // Xóa giỏ hàng của người dùng
        Task<bool> DeleteAsync(int userId);
    }
}
