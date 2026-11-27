using HeThongBanHang_Comestic_UI.Models;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize(Roles = "Customer")]
    public class WishlistController : Controller
    {
        private readonly IWishlistApiService _wishlistService;
        private readonly IProductApiService _productService;

        public WishlistController(
            IWishlistApiService wishlistService,
            IProductApiService productService)
        {
            _wishlistService = wishlistService;
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var list = await _wishlistService.GetMyWishlistAsync();
            var existing = list.FirstOrDefault(x => x.ProductId == productId);

            bool liked;
            if (existing == null)
            {
                await _wishlistService.AddAsync(productId);
                liked = true;
            }
            else
            {
                await _wishlistService.DeleteAsync(existing.Id);
                liked = false;
            }

            var updated = await _wishlistService.GetMyWishlistAsync();
            return Json(new { success = true, liked, totalCount = updated.Count });
        }

        public async Task<IActionResult> Index()
        {
            var wishlists = await _wishlistService.GetMyWishlistAsync();
            var products = await _productService.GetAllAsync();

            var data = wishlists.Select(w =>
            {
                var p = products.FirstOrDefault(x => x.Id == w.ProductId);
                return new WishlistItemVM
                {
                    Id = w.Id,
                    ProductId = w.ProductId,
                    ProductName = p?.ProductName ?? $"Sản phẩm #{w.ProductId}",
                    Picture = p?.Picture ?? "",
                    UnitPrice = p?.UnitPrice ?? 0
                };
            }).ToList();

            return View(data);
        }
    }
}