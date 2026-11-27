using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.ViewComponents
{
    public class OrderCountViewComponent : ViewComponent
    {
        private readonly IOrderApiService _orderService;

        public OrderCountViewComponent(IOrderApiService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (HttpContext.User.Identity?.IsAuthenticated != true)
            {
                return View(0);
            }

            int count = await _orderService.GetBadgeCountAsync();
            return View(count);
        }
    }
}
