
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public WishlistService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy danh sách sản phẩm yêu thích của người dùng
        public async Task<List<WishlistResponseDTO>> GetMyWishlistAsync(int userId)
        {
            var wishlists = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<WishlistResponseDTO>>(wishlists);
        }

        // Thêm sản phẩm vào danh sách yêu thích
        public async Task<bool> CreateAsync(WishlistCreateDTO dto, int userId)
        {
            // Kiểm tra sản phẩm có tồn tại và đang hoạt động không
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == dto.ProductId && p.Status == 1);

            if (!productExists)
            {
                return false;
            }

            // Kiểm tra sản phẩm đã có trong Wishlist chưa
            var alreadyExists = await _context.Wishlists
                .AnyAsync(w => w.UserId == userId &&
                               w.ProductId == dto.ProductId);

            if (alreadyExists)
            {
                return false;
            }

            // Ánh xạ DTO sang Model
            var wishlist = _mapper.Map<Wishlist>(dto);

            // Gán UserId từ JWT
            wishlist.UserId = userId;

            // Gán thời gian tạo
            wishlist.CreatedAt = DateTime.Now;

            _context.Wishlists.Add(wishlist);

            await _context.SaveChangesAsync();

            return true;
        }

        // Xóa sản phẩm khỏi danh sách yêu thích
        public async Task<bool> DeleteAsync(int id, int userId)
        {
            // Chỉ tìm Wishlist thuộc về người dùng đang đăng nhập
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == id &&
                                          w.UserId == userId);

            if (wishlist == null)
            {
                return false;
            }

            _context.Wishlists.Remove(wishlist);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
