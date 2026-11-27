using HeThongBanHang_Comestic_UI.Models;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeThongBanHang_Comestic_UI.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly ICartApiService _cartService;
        public CartViewComponent(ICartApiService cartService)
        {
            _cartService = cartService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (HttpContext.User.Identity?.IsAuthenticated != true || !HttpContext.User.IsInRole("Customer"))
            {
                return View("CartPanel", new CartModel { quantity = 0, total = 0 });
            }
            var items = await _cartService.GetMyCartItemsAsync();
            return View("CartPanel", new CartModel
            {
                quantity = items.Sum(x => x.Quantity),
                total = items.Sum(x => x.Quantity * x.Price)
            });
        }
    }
}