using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class ProductService : IProductService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public ProductService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // =====================================
        // LẤY DANH SÁCH SẢN PHẨM
        // =====================================
        public async Task<List<ProductResponseDTO>> GetAllAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p =>
                    p.Status == 1 &&
                    (p.CategoryId == null ||
                     (p.Category != null && p.Category.Status == 1)))
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ProductResponseDTO>>(products);
        }

        // =====================================
        // LẤY CHI TIẾT SẢN PHẨM THEO ID
        // =====================================
        public async Task<ProductResponseDTO?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.Status == 1 &&
                    (p.CategoryId == null ||
                     (p.Category != null && p.Category.Status == 1)));

            if (product == null)
                return null;

            return _mapper.Map<ProductResponseDTO>(product);
        }

        // =====================================
        // LẤY SẢN PHẨM THEO DANH MỤC
        // =====================================
        public async Task<List<ProductResponseDTO>> GetByCategoryAsync(int categoryId)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p =>
                    p.CategoryId == categoryId &&
                    p.Status == 1 &&
                    p.Category != null &&
                    p.Category.Status == 1)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ProductResponseDTO>>(products);
        }

        // =====================================
        // THÊM SẢN PHẨM
        // =====================================
        public async Task<bool> CreateAsync(ProductCreateDTO dto, int userId)
        {
            // Kiểm tra tên sản phẩm
            if (string.IsNullOrWhiteSpace(dto.ProductName))
                return false;

            // Kiểm tra giá không được âm
            if (dto.UnitPrice < 0)
                return false;

            // Kiểm tra số lượng không được âm
            if (dto.UnitsInStock < 0 ||
                dto.UnitsOnOrder < 0 ||
                dto.ReorderLevel < 0)
                return false;

            // Kiểm tra danh mục có tồn tại và đang hoạt động không
            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories
                    .AnyAsync(c =>
                        c.Id == dto.CategoryId.Value &&
                        c.Status == 1);

                if (!categoryExists)
                    return false;
            }

            // Kiểm tra nhà cung cấp có tồn tại và đang hoạt động không
            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s =>
                        s.Id == dto.SupplierId.Value &&
                        s.Status == 1);

                if (!supplierExists)
                    return false;
            }

            var product = _mapper.Map<Product>(dto);

            product.ProductName = dto.ProductName.Trim();
            product.CreatedAt = DateTime.Now;
            product.CreatedBy = userId;
            product.Status = 1;

            _context.Products.Add(product);

            return await _context.SaveChangesAsync() > 0;
        }

        // =====================================
        // CẬP NHẬT SẢN PHẨM
        // =====================================
        public async Task<bool> UpdateAsync(int id, ProductUpdateDTO dto, int userId)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return false;

            if (string.IsNullOrWhiteSpace(dto.ProductName))
                return false;

            if (dto.UnitPrice < 0)
                return false;

            if (dto.UnitsInStock < 0 ||
                dto.UnitsOnOrder < 0 ||
                dto.ReorderLevel < 0)
                return false;

            // Kiểm tra danh mục
            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories
                    .AnyAsync(c =>
                        c.Id == dto.CategoryId.Value &&
                        c.Status == 1);

                if (!categoryExists)
                    return false;
            }

            // Kiểm tra nhà cung cấp
            if (dto.SupplierId.HasValue)
            {
                var supplierExists = await _context.Suppliers
                    .AnyAsync(s =>
                        s.Id == dto.SupplierId.Value &&
                        s.Status == 1);

                if (!supplierExists)
                    return false;
            }

            _mapper.Map(dto, product);

            product.ProductName = dto.ProductName.Trim();
            product.UpdatedAt = DateTime.Now;
            product.UpdatedBy = userId;

            return await _context.SaveChangesAsync() > 0;
        }

        // =====================================
        // ẨN / HIỆN SẢN PHẨM
        // =====================================
        public async Task<bool> UpdateStatusAsync(int id, int status, int userId)
        {
            // Chỉ chấp nhận 0 hoặc 1
            if (status != 0 && status != 1)
                return false;

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return false;

            product.Status = status;
            product.UpdatedAt = DateTime.Now;
            product.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }

        // =====================================
        // LỌC SẢN PHẨM THEO GIÁ
        // =====================================
        public async Task<List<ProductResponseDTO>> FilterByPriceAsync(
            decimal? minPrice,
            decimal? maxPrice)
        {
            // Kiểm tra giá không được âm
            if (minPrice < 0 || maxPrice < 0)
                return new List<ProductResponseDTO>();

            // Giá thấp nhất không được lớn hơn giá cao nhất
            if (minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice.Value > maxPrice.Value)
            {
                return new List<ProductResponseDTO>();
            }

            // Truy vấn sản phẩm đang hoạt động
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p =>
                    p.Status == 1 &&
                    (p.CategoryId == null ||
                     (p.Category != null && p.Category.Status == 1)));

            // Lọc theo giá thấp nhất
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.UnitPrice >= minPrice.Value);
            }

            // Lọc theo giá cao nhất
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.UnitPrice <= maxPrice.Value);
            }

            var products = await query
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ProductResponseDTO>>(products);
        }
    }
}
