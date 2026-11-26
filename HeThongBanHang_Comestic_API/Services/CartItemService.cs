
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public CartItemService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy danh sách sản phẩm trong giỏ hàng của người dùng
        public async Task<List<CartItemResponseDTO>> GetMyCartItemsAsync(int userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return new List<CartItemResponseDTO>();

            var cartItems = await _context.CartItems
                .Where(x => x.CartId == cart.Id)
                .ToListAsync();

            return _mapper.Map<List<CartItemResponseDTO>>(cartItems);
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<bool> CreateAsync(
            CartItemCreateDTO dto,
            int userId)
        {
            if (dto.Quantity <= 0)
                return false;

            // Kiểm tra biến thể sản phẩm
            var variant = await _context.ProductVariants
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.ProductVariantId &&
                    x.Status == 1);

            if (variant == null ||
                variant.Product == null ||
                variant.Product.Status != 1 ||
                !variant.Price.HasValue ||
                variant.Price.Value <= 0)
            {
                return false;
            }

            // Kiểm tra tồn kho
            // StockQuantity phải có giá trị và lớn hơn 0
            if (!variant.StockQuantity.HasValue ||
                variant.StockQuantity.Value <= 0)
            {
                return false;
            }

            // Số lượng thêm không được vượt tồn kho
            if (dto.Quantity > variant.StockQuantity.Value)
                return false;

            // Tìm giỏ hàng của người dùng
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // Nếu chưa có giỏ hàng thì tự động tạo
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Carts.Add(cart);

                await _context.SaveChangesAsync();
            }

            // Kiểm tra sản phẩm đã tồn tại trong giỏ chưa
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(x =>
                    x.CartId == cart.Id &&
                    x.ProductVariantId == dto.ProductVariantId);

            if (existingItem != null)
            {
                // Cộng số lượng mới vào số lượng cũ
                int newQuantity = existingItem.Quantity + dto.Quantity;

                // Không được vượt quá tồn kho
                if (newQuantity > variant.StockQuantity.Value)
                    return false;

                existingItem.Quantity = newQuantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = variant.Id,
                    Quantity = dto.Quantity,
                    Price = variant.Price.Value,
                    ImageUrl = variant.ImageUrl,
                    CreatedAt = DateTime.Now
                };

                _context.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        public async Task<bool> UpdateAsync(
            int id,
            CartItemUpdateDTO dto,
            int userId)
        {
            if (dto.Quantity <= 0)
                return false;

            // Tìm sản phẩm thuộc giỏ hàng của người dùng
            var cartItem = await _context.CartItems
                .Include(x => x.Cart)
                .Include(x => x.ProductVariant)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.Cart.UserId == userId);

            if (cartItem == null)
                return false;

            var variant = cartItem.ProductVariant;

            // Kiểm tra biến thể và sản phẩm còn hoạt động
            if (variant.Status != 1 ||
                variant.Product == null ||
                variant.Product.Status != 1)
            {
                return false;
            }

            // Kiểm tra tồn kho
            if (!variant.StockQuantity.HasValue ||
                variant.StockQuantity.Value <= 0)
            {
                return false;
            }

            // Số lượng cập nhật không được vượt tồn kho
            if (dto.Quantity > variant.StockQuantity.Value)
                return false;

            // Cập nhật số lượng
            cartItem.Quantity = dto.Quantity;
            cartItem.Cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var cartItem = await _context.CartItems
                .Include(x => x.Cart)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.Cart.UserId == userId);

            if (cartItem == null)
                return false;

            cartItem.Cart.UpdatedAt = DateTime.Now;

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
