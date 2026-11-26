
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public CategoryService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // =========================================
        // 1. LẤY DANH SÁCH DANH MỤC (CUSTOMER)
        // =========================================
        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Status == 1)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return _mapper.Map<List<CategoryResponseDTO>>(categories);
        }

        // =========================================
        // 2. THÊM DANH MỤC (ADMIN, STAFF)
        // =========================================
        public async Task<bool> CreateAsync(
            CategoryCreateDTO dto,
            int userId)
        {
            var category = _mapper.Map<Category>(dto);

            category.CategoryName = dto.CategoryName.Trim();
            category.CreatedAt = DateTime.Now;
            category.CreatedBy = userId;
            category.Status = 1;

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return true;
        }

        // =========================================
        // 3. CẬP NHẬT DANH MỤC (ADMIN, STAFF)
        // =========================================
        public async Task<bool> UpdateAsync(
            int id,
            CategoryUpdateDTO dto,
            int userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            // Map DTO vào Category đang tồn tại
            _mapper.Map(dto, category);

            category.CategoryName = dto.CategoryName.Trim();
            category.UpdatedAt = DateTime.Now;
            category.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }

        // =========================================
        // 4. ẨN / HIỆN DANH MỤC (ADMIN, STAFF)
        // =========================================
        public async Task<bool> UpdateStatusAsync(
            int id,
            int status,
            int userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            category.Status = status;
            category.UpdatedAt = DateTime.Now;
            category.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
