using HeThongBanHang_Comestic_UI.DTOs;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Models
{
    public class DashboardVM
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }

        public decimal CompletedRevenue { get; set; }
        public decimal ExpectedRevenue { get; set; }

        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int HiddenProducts { get; set; }

        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }

        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }

        public int MyEmployeeOrdersCount { get; set; }
        public bool IsAdmin { get; set; }

        public List<OrderResponseDTO> RecentOrders { get; set; } = new();
    }
}
