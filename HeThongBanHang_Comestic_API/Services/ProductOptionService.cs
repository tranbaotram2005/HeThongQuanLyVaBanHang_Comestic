
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class ProductOptionService : IProductOptionService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public ProductOptionService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // =====================================
        // LẤY DANH SÁCH THUỘC TÍNH THEO SẢN PHẨM
        // =====================================
        public async Task<List<ProductOptionResponseDTO>> GetByProductAsync(int productId)
        {
            var options = await _context.ProductOptions
                .Where(o =>
                    o.ProductId == productId &&
                    o.Status == 1)
                .OrderBy(o => o.SortOrder)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ProductOptionResponseDTO>>(options);
        }

        // =====================================
        // LẤY CHI TIẾT THUỘC TÍNH
        // =====================================
        public async Task<ProductOptionResponseDTO?> GetByIdAsync(int id)
        {
            var option = await _context.ProductOptions
                .AsNoTracking()
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.Status == 1);

            if (option == null)
                return null;

            return _mapper.Map<ProductOptionResponseDTO>(option);
        }

        // =====================================
        // THÊM THUỘC TÍNH
        // =====================================
        public async Task<bool> CreateAsync(ProductOptionCreateDTO dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return false;

            // Kiểm tra sản phẩm tồn tại và đang hoạt động
            var productExists = await _context.Products
                .AnyAsync(p =>
                    p.Id == dto.ProductId &&
                    p.Status == 1);

            if (!productExists)
                return false;

            // Không cho phép trùng tên thuộc tính trong cùng sản phẩm
            var name = dto.Name.Trim();

            var nameExists = await _context.ProductOptions
                .AnyAsync(o =>
                    o.ProductId == dto.ProductId &&
                    o.Name.ToLower() == name.ToLower() &&
                    o.Status == 1);

            if (nameExists)
                return false;

            var option = _mapper.Map<ProductOption>(dto);

            option.Name = name;
            option.CreatedAt = DateTime.Now;
            option.CreatedBy = userId;
            option.Status = 1;

            _context.ProductOptions.Add(option);

            return await _context.SaveChangesAsync() > 0;
        }

        // =====================================
        // CẬP NHẬT THUỘC TÍNH
        // =====================================
        public async Task<bool> UpdateAsync(int id, ProductOptionUpdateDTO dto, int userId)
        {
            var option = await _context.ProductOptions.FindAsync(id);

            if (option == null || option.Status != 1)
                return false;

            if (string.IsNullOrWhiteSpace(dto.Name))
                return false;

            var name = dto.Name.Trim();

            // Kiểm tra trùng tên trong cùng sản phẩm
            var nameExists = await _context.ProductOptions
                .AnyAsync(o =>
                    o.Id != id &&
                    o.ProductId == option.ProductId &&
                    o.Name.ToLower() == name.ToLower() &&
                    o.Status == 1);

            if (nameExists)
                return false;

            _mapper.Map(dto, option);

            option.Name = name;
            option.UpdatedAt = DateTime.Now;
            option.UpdatedBy = userId;

            return await _context.SaveChangesAsync() > 0;
        }

        // =====================================
        // ẨN / HIỆN THUỘC TÍNH
        // =====================================
        public async Task<bool> UpdateStatusAsync(int id, int status, int userId)
        {
            if (status != 0 && status != 1)
                return false;

            var option = await _context.ProductOptions.FindAsync(id);

            if (option == null)
                return false;

            option.Status = status;
            option.UpdatedAt = DateTime.Now;
            option.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
