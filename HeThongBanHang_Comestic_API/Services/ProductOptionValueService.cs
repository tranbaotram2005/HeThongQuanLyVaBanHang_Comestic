
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class ProductOptionValueService : IProductOptionValueService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public ProductOptionValueService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // =====================================
        // LẤY DANH SÁCH GIÁ TRỊ THEO THUỘC TÍNH
        // =====================================
        public async Task<List<ProductOptionValueResponseDTO>> GetByOptionAsync(int productOptionId)
        {
            var values = await _context.ProductOptionValues
                .Where(v =>
                    v.ProductOptionId == productOptionId &&
                    v.Status == 1 &&
                    v.ProductOption != null &&
                    v.ProductOption.Status == 1)
                .OrderBy(v => v.SortOrder)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ProductOptionValueResponseDTO>>(values);
        }

        // =====================================
        // LẤY CHI TIẾT GIÁ TRỊ THEO ID
        // =====================================
        public async Task<ProductOptionValueResponseDTO?> GetByIdAsync(int id)
        {
            var value = await _context.ProductOptionValues
                .AsNoTracking()
                .FirstOrDefaultAsync(v =>
                    v.Id == id &&
                    v.Status == 1 &&
                    v.ProductOption != null &&
                    v.ProductOption.Status == 1);

            if (value == null)
                return null;

            return _mapper.Map<ProductOptionValueResponseDTO>(value);
        }

        // =====================================
        // THÊM GIÁ TRỊ THUỘC TÍNH
        // =====================================
        public async Task<bool> CreateAsync(ProductOptionValueCreateDTO dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return false;

            // Kiểm tra thuộc tính cha tồn tại và đang hoạt động
            var optionExists = await _context.ProductOptions
                .AnyAsync(o =>
                    o.Id == dto.ProductOptionId &&
                    o.Status == 1 &&
                    o.Product.Status == 1);

            if (!optionExists)
                return false;

            var name = dto.Name.Trim();

            // Không cho trùng tên trong cùng một thuộc tính
            var nameExists = await _context.ProductOptionValues
                .AnyAsync(v =>
                    v.ProductOptionId == dto.ProductOptionId &&
                    v.Name.ToLower() == name.ToLower() &&
                    v.Status == 1);

            if (nameExists)
                return false;

            var value = _mapper.Map<ProductOptionValue>(dto);

            value.Name = name;
            value.CreatedAt = DateTime.Now;
            value.CreatedBy = userId;
            value.Status = 1;

            _context.ProductOptionValues.Add(value);

            return await _context.SaveChangesAsync() > 0;
        }

        // =====================================
        // CẬP NHẬT GIÁ TRỊ THUỘC TÍNH
        // =====================================
        public async Task<bool> UpdateAsync(int id, ProductOptionValueUpdateDTO dto, int userId)
        {
            var value = await _context.ProductOptionValues.FindAsync(id);

            if (value == null || value.Status != 1)
                return false;

            if (string.IsNullOrWhiteSpace(dto.Name))
                return false;

            var name = dto.Name.Trim();

            // Kiểm tra trùng tên trong cùng thuộc tính
            var nameExists = await _context.ProductOptionValues
                .AnyAsync(v =>
                    v.Id != id &&
                    v.ProductOptionId == value.ProductOptionId &&
                    v.Name.ToLower() == name.ToLower() &&
                    v.Status == 1);

            if (nameExists)
                return false;

            _mapper.Map(dto, value);

            value.Name = name;
            value.UpdatedAt = DateTime.Now;
            value.UpdatedBy = userId;

            return await _context.SaveChangesAsync() > 0;
        }

        // =====================================
        // ẨN / HIỆN GIÁ TRỊ THUỘC TÍNH
        // =====================================
        public async Task<bool> UpdateStatusAsync(int id, int status, int userId)
        {
            if (status != 0 && status != 1)
                return false;

            var value = await _context.ProductOptionValues.FindAsync(id);

            if (value == null)
                return false;

            value.Status = status;
            value.UpdatedAt = DateTime.Now;
            value.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
