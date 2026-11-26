
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class CartService : ICartService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public CartService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy giỏ hàng của người dùng
        public async Task<CartResponseDTO?> GetMyCartAsync(int userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId);

            return cart == null
                ? null
                : _mapper.Map<CartResponseDTO>(cart);
        }

        // Tạo giỏ hàng mới
        public async Task<bool> CreateAsync(CartCreateDTO dto, int userId)
        {
            // Mỗi người dùng chỉ có một giỏ hàng
            bool cartExists = await _context.Carts
                .AnyAsync(x => x.UserId == userId);

            if (cartExists)
                return false;

            var cart = _mapper.Map<Cart>(dto);

            cart.UserId = userId;
            cart.CreatedAt = DateTime.Now;
            cart.UpdatedAt = DateTime.Now;

            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();

            return true;
        }

        // Cập nhật mã giảm giá và ghi chú
        public async Task<bool> UpdateAsync(CartUpdateDTO dto, int userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
                return false;

            _mapper.Map(dto, cart);

            cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        // Xóa giỏ hàng
        public async Task<bool> DeleteAsync(int userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
                return false;

            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
