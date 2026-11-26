
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using HeThongBanHang_Comestic_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public InventoryService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Lấy danh sách tồn kho đang hoạt động
        public async Task<List<InventoryResponseDTO>> GetAllAsync()
        {
            var inventories = await _context.Inventories
                .Where(i => i.Status == 1)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<InventoryResponseDTO>>(inventories);
        }

        // Lấy tồn kho theo ID
        public async Task<InventoryResponseDTO?> GetByIdAsync(int id)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == id && i.Status == 1);

            if (inventory == null)
            {
                return null;
            }

            return _mapper.Map<InventoryResponseDTO>(inventory);
        }

        // Lấy tồn kho theo biến thể
        public async Task<List<InventoryResponseDTO>> GetByVariantAsync(int variantId)
        {
            var inventories = await _context.Inventories
                .Where(i => i.VariantId == variantId && i.Status == 1)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<InventoryResponseDTO>>(inventories);
        }

        // Tạo bản ghi tồn kho
        public async Task<bool> CreateAsync(
            InventoryCreateDTO dto,
            int userId)
        {
            if (dto.Quantity < 0)
            {
                return false;
            }

            var variantExists = await _context.ProductVariants
                .AnyAsync(v => v.Id == dto.VariantId && v.Status == 1);

            if (!variantExists)
            {
                return false;
            }

            var alreadyExists = await _context.Inventories
                .AnyAsync(i => i.VariantId == dto.VariantId
                    && i.WarehouseId == dto.WarehouseId
                    && i.Status == 1);

            if (alreadyExists)
            {
                return false;
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var inventory = _mapper.Map<Inventory>(dto);

                inventory.CreatedAt = DateTime.Now;
                inventory.CreatedBy = userId;
                inventory.Status = 1;

                _context.Inventories.Add(inventory);

                await _context.SaveChangesAsync();

                // Đồng bộ tổng tồn kho của biến thể
                await SyncVariantStockAsync(dto.VariantId);

                await transaction.CommitAsync();

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Cập nhật thông tin tồn kho
        public async Task<bool> UpdateAsync(
            int id,
            InventoryUpdateDTO dto,
            int userId)
        {
            if (dto.Quantity < 0)
            {
                return false;
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == id && i.Status == 1);

            if (inventory == null)
            {
                return false;
            }

            var alreadyExists = await _context.Inventories
                .AnyAsync(i => i.Id != id
                    && i.VariantId == inventory.VariantId
                    && i.WarehouseId == dto.WarehouseId
                    && i.Status == 1);

            if (alreadyExists)
            {
                return false;
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _mapper.Map(dto, inventory);

                inventory.UpdatedAt = DateTime.Now;
                inventory.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                // Tính lại tổng tồn kho
                await SyncVariantStockAsync(inventory.VariantId);

                await transaction.CommitAsync();

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Cập nhật trạng thái tồn kho
        public async Task<bool> UpdateStatusAsync(
            int id,
            int status,
            int userId)
        {
            if (status != 0 && status != 1)
            {
                return false;
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventory == null)
            {
                return false;
            }

            if (inventory.Status == status)
            {
                return true;
            }

            if (status == 1)
            {
                var alreadyExists = await _context.Inventories
                    .AnyAsync(i => i.Id != id
                        && i.VariantId == inventory.VariantId
                        && i.WarehouseId == inventory.WarehouseId
                        && i.Status == 1);

                if (alreadyExists)
                {
                    return false;
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                inventory.Status = status;
                inventory.UpdatedAt = DateTime.Now;
                inventory.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                // Đồng bộ lại tổng tồn kho
                await SyncVariantStockAsync(inventory.VariantId);

                await transaction.CommitAsync();

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Đồng bộ StockQuantity theo tổng Inventory đang hoạt động
        private async Task SyncVariantStockAsync(int variantId)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);

            if (variant == null)
            {
                return;
            }

            var totalQuantity = await _context.Inventories
                .Where(i => i.VariantId == variantId && i.Status == 1)
                .SumAsync(i => i.Quantity ?? 0);

            variant.StockQuantity = totalQuantity;

            await _context.SaveChangesAsync();
        }
    }
}
