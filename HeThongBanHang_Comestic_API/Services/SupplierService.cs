using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public SupplierService(WebApiGtEcommerceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SupplierResponseDTO>> GetAllAsync()
        {
            var suppliers = await _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Status == 1)
                .OrderBy(s => s.CompanyName)
                .ToListAsync();

            return _mapper.Map<List<SupplierResponseDTO>>(suppliers);
        }

        public async Task<SupplierResponseDTO?> GetByIdAsync(int id)
        {
            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Status == 1);

            return supplier == null ? null : _mapper.Map<SupplierResponseDTO>(supplier);
        }

        public async Task<bool> CreateAsync(SupplierCreateDTO dto, int userId)
        {
            var supplier = _mapper.Map<Supplier>(dto);
            supplier.CompanyName = dto.CompanyName.Trim();
            supplier.CreatedAt = DateTime.Now;
            supplier.CreatedBy = userId;
            supplier.Status = 1;

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(int id, SupplierUpdateDTO dto, int userId)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return false;

            _mapper.Map(dto, supplier);
            supplier.CompanyName = dto.CompanyName.Trim();
            supplier.UpdatedAt = DateTime.Now;
            supplier.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, int status, int userId)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return false;

            supplier.Status = status;
            supplier.UpdatedAt = DateTime.Now;
            supplier.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SupplierResponseDTO>> GetAllForAdminAsync()
        {
            var suppliers = await _context.Suppliers
                .AsNoTracking()
                .OrderByDescending(s => s.Status)
                .ThenBy(s => s.CompanyName)
                .ToListAsync();

            return _mapper.Map<List<SupplierResponseDTO>>(suppliers);
        }
    }
}