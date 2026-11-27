using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.ViewComponents
{
    public class BrandsViewComponent : ViewComponent
    {
        private readonly ISupplierApiService _supplierService;

        public BrandsViewComponent(ISupplierApiService supplierService)
        {
            _supplierService = supplierService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var suppliers = await _supplierService.GetAllAsync();
            return View("Default", suppliers);
        }
    }
}