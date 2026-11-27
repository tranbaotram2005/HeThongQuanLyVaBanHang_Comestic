using HeThongBanHang_Comestic_UI.Models;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace HeThongBanHang_Comestic_UI.ViewComponents
{
    public class WishlistCountViewComponent : ViewComponent
    {
        private readonly IWishlistApiService _wishlistService;
        public WishlistCountViewComponent(IWishlistApiService wishlistService)
        {
            _wishlistService = wishlistService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (HttpContext.User.Identity?.IsAuthenticated != true || !HttpContext.User.IsInRole("Customer"))
            {
                return View(0);
            }
            var list = await _wishlistService.GetMyWishlistAsync();
            return View(list.Count);
        }
    }
}