
using AutoMapper;
using HeThongBanHang_Comestic_API.Data;
using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang_Comestic_API.Services
{
    public class ShipInfoService : IShipInfoService
    {
        private readonly WebApiGtEcommerceContext _context;
        private readonly IMapper _mapper;

        public ShipInfoService(
            WebApiGtEcommerceContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // =====================================================
        // 1. CUSTOMER - XEM THÔNG TIN GIAO HÀNG ĐƠN CỦA MÌNH
        // =====================================================
        public async Task<List<ShipInfoResponseDTO>> GetMyShipInfosAsync(
            int userId, int orderId)
        {
            // Tìm Customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.Status == 1);

            if (customer == null)
                return new List<ShipInfoResponseDTO>();

            // Kiểm tra đơn thuộc Customer
            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.CustomerId == customer.Id);

            if (order == null)
                return new List<ShipInfoResponseDTO>();

            // Lấy ShipInfo
            var shipInfos = await _context.ShipInfos
                .Where(s => s.OrderId == orderId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<ShipInfoResponseDTO>>(shipInfos);
        }

        // =====================================================
        // 2. ADMIN, EMPLOYEE - XEM THEO ORDER ID
        // =====================================================
        public async Task<List<ShipInfoResponseDTO>>
            GetShipInfosByOrderIdAsync(int orderId)
        {
            bool orderExists = await _context.Orders
                .AnyAsync(o => o.Id == orderId);

            if (!orderExists)
                return new List<ShipInfoResponseDTO>();

            var shipInfos = await _context.ShipInfos
                .Where(s => s.OrderId == orderId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<ShipInfoResponseDTO>>(shipInfos);
        }

        // =====================================================
        // 3. ADMIN, EMPLOYEE - XEM TẤT CẢ
        // =====================================================
        public async Task<List<ShipInfoResponseDTO>> GetAllShipInfosAsync()
        {
            var shipInfos = await _context.ShipInfos
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<ShipInfoResponseDTO>>(shipInfos);
        }

        // =====================================================
        // 4. ADMIN, EMPLOYEE - XEM CHI TIẾT
        // =====================================================
        public async Task<ShipInfoResponseDTO?> GetShipInfoByIdAsync(
            int shipInfoId)
        {
            var shipInfo = await _context.ShipInfos
                .FirstOrDefaultAsync(s => s.Id == shipInfoId);

            if (shipInfo == null)
                return null;

            return _mapper.Map<ShipInfoResponseDTO>(shipInfo);
        }

        // =====================================================
        // 5. ADMIN, EMPLOYEE - CẬP NHẬT THÔNG TIN GIAO HÀNG
        // =====================================================
        public async Task<bool> UpdateShipInfoAsync(
            int shipInfoId,
            ShipInfoUpdateDTO dto,
            int userId)
        {
            // Tìm ShipInfo
            var shipInfo = await _context.ShipInfos
                .FirstOrDefaultAsync(s => s.Id == shipInfoId);

            if (shipInfo == null)
                return false;

            // Không cập nhật ShipInfo đã giao thành công
            if (shipInfo.Status == 3)
                throw new Exception(
                    "Đơn hàng đã giao thành công, không thể cập nhật.");

            // Kiểm tra Shipper
            var shipper = await _context.Shippers
                .FirstOrDefaultAsync(s =>
                    s.Id == dto.ShipperId &&
                    s.Status == 1);

            if (shipper == null)
                throw new Exception("Đơn vị vận chuyển không hợp lệ.");

            // Kiểm tra trạng thái
            if (dto.Status.HasValue &&
                (dto.Status.Value < 0 || dto.Status.Value > 4))
            {
                throw new Exception("Trạng thái vận chuyển không hợp lệ.");
            }

            // Không cho chuyển trạng thái đã hủy về trạng thái khác
            if (shipInfo.Status == 4 &&
                dto.Status.HasValue &&
                dto.Status.Value != 4)
            {
                throw new Exception(
                    "Thông tin giao hàng đã hủy, không thể khôi phục.");
            }

            // Cập nhật Shipper
            shipInfo.ShipperId = dto.ShipperId;

            // Cập nhật ngày giao dự kiến
            shipInfo.RequiredDate = dto.RequiredDate;

            // Cập nhật ngày gửi
            shipInfo.ShippedDate = dto.ShippedDate;

            // Cập nhật địa chỉ
            shipInfo.ShipAddress = dto.ShipAddress;
            shipInfo.ShipName = dto.ShipName;
            shipInfo.ShipPhone = dto.ShipPhone;
            shipInfo.ShipCity = dto.ShipCity;
            shipInfo.ShipRegion = dto.ShipRegion;
            shipInfo.ShipPostalCode = dto.ShipPostalCode;
            shipInfo.ShipCountry = dto.ShipCountry;

            // Cập nhật ghi chú
            shipInfo.Notes = dto.Notes;

            // Cập nhật trạng thái nếu được gửi lên
            if (dto.Status.HasValue)
            {
                shipInfo.Status = dto.Status.Value;
            }

            // Nếu chuyển sang trạng thái đang vận chuyển
            // mà chưa có ngày gửi thì tự động ghi ngày hiện tại
            if (shipInfo.Status == 2 &&
                shipInfo.ShippedDate == null)
            {
                shipInfo.ShippedDate = DateTime.Now;
            }

            // Cập nhật thông tin người sửa
            shipInfo.UpdatedAt = DateTime.Now;
            shipInfo.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
