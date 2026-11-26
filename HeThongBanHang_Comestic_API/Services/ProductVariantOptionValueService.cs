
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class ProductVariantOptionValueService : IProductVariantOptionValueService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public ProductVariantOptionValueService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy danh sách giá trị thuộc tính theo biến thể
        public async Task<List<ProductVariantOptionValueResponseDTO>> GetByVariantAsync(int productVariantId)
        {
            var items = await _context.ProductVariantOptionValues
                .Where(x => x.ProductVariantId == productVariantId && x.Status == 1)
                .OrderBy(x => x.Id)
                .ToListAsync();

            return _mapper.Map<List<ProductVariantOptionValueResponseDTO>>(items);
        }

        // Lấy chi tiết một bản ghi liên kết
        public async Task<ProductVariantOptionValueResponseDTO?> GetByIdAsync(int id)
        {
            var item = await _context.ProductVariantOptionValues
                .FirstOrDefaultAsync(x => x.Id == id && x.Status == 1);

            return item == null
                ? null
                : _mapper.Map<ProductVariantOptionValueResponseDTO>(item);
        }

        // Thêm giá trị thuộc tính vào biến thể
        public async Task<bool> CreateAsync(ProductVariantOptionValueCreateDTO dto, int userId)
        {
            // Tìm biến thể sản phẩm
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.ProductVariantId &&
                    x.Status == 1);

            if (variant == null || variant.ProductId == null)
                return false;

            // Tìm giá trị thuộc tính
            var optionValue = await _context.ProductOptionValues
                .Include(x => x.ProductOption)
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.ProductOptionValueId &&
                    x.Status == 1);

            if (optionValue == null)
                return false;

            // Kiểm tra giá trị thuộc tính có thuộc sản phẩm của biến thể không
            if (optionValue.ProductOption.ProductId != variant.ProductId)
                return false;

            // Không cho phép gán trùng giá trị thuộc tính cho cùng biến thể
            bool exists = await _context.ProductVariantOptionValues
                .AnyAsync(x =>
                    x.ProductVariantId == dto.ProductVariantId &&
                    x.ProductOptionValueId == dto.ProductOptionValueId &&
                    x.Status == 1);

            if (exists)
                return false;

            var item = _mapper.Map<ProductVariantOptionValue>(dto);

            item.CreatedAt = DateTime.Now;
            item.CreatedBy = userId;
            item.Status = 1;

            await _context.ProductVariantOptionValues.AddAsync(item);
            await _context.SaveChangesAsync();

            return true;
        }

        // Cập nhật giá trị thuộc tính của biến thể
        public async Task<bool> UpdateAsync(int id, ProductVariantOptionValueUpdateDTO dto, int userId)
        {
            var item = await _context.ProductVariantOptionValues
                .FirstOrDefaultAsync(x => x.Id == id && x.Status == 1);

            if (item == null)
                return false;

            // Tìm biến thể hiện tại
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x =>
                    x.Id == item.ProductVariantId &&
                    x.Status == 1);

            if (variant == null || variant.ProductId == null)
                return false;

            // Tìm giá trị thuộc tính mới
            var optionValue = await _context.ProductOptionValues
                .Include(x => x.ProductOption)
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.ProductOptionValueId &&
                    x.Status == 1);

            if (optionValue == null)
                return false;

            // Kiểm tra giá trị mới có thuộc sản phẩm của biến thể không
            if (optionValue.ProductOption.ProductId != variant.ProductId)
                return false;

            // Không cho phép trùng với bản ghi khác
            bool exists = await _context.ProductVariantOptionValues
                .AnyAsync(x =>
                    x.ProductVariantId == item.ProductVariantId &&
                    x.ProductOptionValueId == dto.ProductOptionValueId &&
                    x.Id != id &&
                    x.Status == 1);

            if (exists)
                return false;

            _mapper.Map(dto, item);

            item.UpdatedAt = DateTime.Now;
            item.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }

        // Ẩn hoặc kích hoạt lại bản ghi liên kết
        public async Task<bool> UpdateStatusAsync(int id, int status, int userId)
        {
            if (status != 0 && status != 1)
                return false;

            var item = await _context.ProductVariantOptionValues
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return false;

            if (status == 1)
            {
                // Kiểm tra biến thể còn hoạt động không
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(x =>
                        x.Id == item.ProductVariantId &&
                        x.Status == 1);

                if (variant == null || variant.ProductId == null)
                    return false;

                // Kiểm tra giá trị thuộc tính còn hoạt động không
                var optionValue = await _context.ProductOptionValues
                    .Include(x => x.ProductOption)
                    .FirstOrDefaultAsync(x =>
                        x.Id == item.ProductOptionValueId &&
                        x.Status == 1);

                if (optionValue == null ||
                    optionValue.ProductOption.ProductId != variant.ProductId)
                    return false;

                // Không kích hoạt nếu đã có bản ghi khác đang sử dụng cặp này
                bool exists = await _context.ProductVariantOptionValues
                    .AnyAsync(x =>
                        x.ProductVariantId == item.ProductVariantId &&
                        x.ProductOptionValueId == item.ProductOptionValueId &&
                        x.Id != id &&
                        x.Status == 1);

                if (exists)
                    return false;
            }

            item.Status = status;
            item.UpdatedAt = DateTime.Now;
            item.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
