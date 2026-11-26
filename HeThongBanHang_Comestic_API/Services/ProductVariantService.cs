
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public ProductVariantService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy danh sách biến thể theo sản phẩm
        public async Task<List<ProductVariantResponseDTO>> GetByProductAsync(int productId)
        {
            var variants = await _context.ProductVariants
                .Where(x => x.ProductId == productId && x.Status == 1)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Id)
                .ToListAsync();

            return _mapper.Map<List<ProductVariantResponseDTO>>(variants);
        }

        // Lấy chi tiết một biến thể
        public async Task<ProductVariantResponseDTO?> GetByIdAsync(int id)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == id && x.Status == 1);

            return variant == null
                ? null
                : _mapper.Map<ProductVariantResponseDTO>(variant);
        }

        // Thêm biến thể mới
        public async Task<bool> CreateAsync(ProductVariantCreateDTO dto, int userId)
        {
            // Kiểm tra sản phẩm có tồn tại và đang hoạt động không
            bool productExists = await _context.Products
                .AnyAsync(x => x.Id == dto.ProductId && x.Status == 1);

            if (!productExists)
                return false;

            // SKU bắt buộc và không được trùng
            if (string.IsNullOrWhiteSpace(dto.Sku))
                return false;

            bool skuExists = await _context.ProductVariants
                .AnyAsync(x => x.Sku == dto.Sku);

            if (skuExists)
                return false;

            // Kiểm tra giá trị không được âm
            if (dto.Price < 0 ||
                dto.CompareAtPrice < 0 ||
                dto.StockQuantity < 0 ||
                dto.Weight < 0 ||
                dto.Length < 0 ||
                dto.Width < 0 ||
                dto.Height < 0)
            {
                return false;
            }

            var variant = _mapper.Map<ProductVariant>(dto);

            variant.CreatedAt = DateTime.Now;
            variant.CreatedBy = userId;
            variant.Status = 1;

            // Nếu biến thể mới là mặc định,
            // bỏ mặc định của các biến thể khác cùng sản phẩm
            if (variant.IsDefault == true)
            {
                var oldDefaults = await _context.ProductVariants
                    .Where(x => x.ProductId == dto.ProductId
                             && x.IsDefault == true
                             && x.Status == 1)
                    .ToListAsync();

                foreach (var item in oldDefaults)
                {
                    item.IsDefault = false;
                    item.UpdatedAt = DateTime.Now;
                    item.UpdatedBy = userId;
                }
            }

            await _context.ProductVariants.AddAsync(variant);
            await _context.SaveChangesAsync();

            return true;
        }

        // Cập nhật biến thể
        public async Task<bool> UpdateAsync(int id, ProductVariantUpdateDTO dto, int userId)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == id && x.Status == 1);

            if (variant == null)
                return false;

            // SKU bắt buộc và không được trùng với biến thể khác
            if (string.IsNullOrWhiteSpace(dto.Sku))
                return false;

            bool skuExists = await _context.ProductVariants
                .AnyAsync(x => x.Sku == dto.Sku && x.Id != id);

            if (skuExists)
                return false;

            // Kiểm tra giá trị không được âm
            if (dto.Price < 0 ||
                dto.CompareAtPrice < 0 ||
                dto.StockQuantity < 0 ||
                dto.Weight < 0 ||
                dto.Length < 0 ||
                dto.Width < 0 ||
                dto.Height < 0)
            {
                return false;
            }

            _mapper.Map(dto, variant);

            variant.UpdatedAt = DateTime.Now;
            variant.UpdatedBy = userId;

            // Nếu biến thể này được chọn làm mặc định,
            // bỏ mặc định của các biến thể khác cùng sản phẩm
            if (variant.IsDefault == true)
            {
                var oldDefaults = await _context.ProductVariants
                    .Where(x => x.ProductId == variant.ProductId
                             && x.Id != id
                             && x.IsDefault == true
                             && x.Status == 1)
                    .ToListAsync();

                foreach (var item in oldDefaults)
                {
                    item.IsDefault = false;
                    item.UpdatedAt = DateTime.Now;
                    item.UpdatedBy = userId;
                }
            }

            await _context.SaveChangesAsync();

            return true;
        }

        // Ẩn hoặc kích hoạt lại biến thể
        public async Task<bool> UpdateStatusAsync(int id, int status, int userId)
        {
            if (status != 0 && status != 1)
                return false;

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == id);

            if (variant == null)
                return false;

            variant.Status = status;
            variant.UpdatedAt = DateTime.Now;
            variant.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
