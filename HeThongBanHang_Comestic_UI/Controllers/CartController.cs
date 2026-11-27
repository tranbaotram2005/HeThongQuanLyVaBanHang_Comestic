using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Models;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ICartApiService _cartService;
        private readonly IProductVariantApiService _variantService;
        private readonly IProductApiService _productService;
        private readonly IAccountApiService _accountService;
        private readonly IOrderApiService _orderService;
        public CartController(ICartApiService cartService,
            IProductVariantApiService variantService,IOrderApiService orderService,IAccountApiService accountApiService,
            IProductApiService productService)
        {
            _cartService = cartService;
            _variantService = variantService;
            _productService = productService;
            _accountService = accountApiService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var apiItems = await _cartService.GetMyCartItemsAsync();
            var items = new List<CartItem>();
            foreach (var x in apiItems)
            {
                var variant = await _variantService.GetByIdAsync(x.ProductVariantId);
                var product = variant?.ProductId != null
                    ? await _productService.GetByIdAsync(variant.ProductId.Value)
                    : null;
                items.Add(new CartItem
                {
                    Id = x.Id,
                    CartId = x.CartId,
                    ProductVariantId = x.ProductVariantId,
                    ImageUrl = x.ImageUrl ?? variant?.ImageUrl ?? "",
                    ProductName = product?.ProductName ?? variant?.Name ?? $"Sản phẩm #{x.ProductVariantId}",
                    VariantText = variant?.Name ?? "",
                    Price = x.Price,
                    Quantity = x.Quantity
                });
            }
            return View(items);
        }

        // =========================
        // ADD TO CART
        // =========================
        public async Task<IActionResult> AddToCart(int variantId, int quantity = 1)
        {
            var (success, message) = await _cartService.AddItemAsync(variantId, quantity);
            if (!success)
                TempData["Error"] = message;
            return RedirectToAction("Index");
        }


        // =========================
        // REMOVE ITEM
        // =========================
        public async Task<IActionResult> RemoveCart(int id)
        {
            await _cartService.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }

        // =========================
        // UPDATE QUANTITY (NEW - NÊN CÓ)
        // =========================
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            if (quantity <= 0)
                await _cartService.DeleteItemAsync(id);
            else
                await _cartService.UpdateItemAsync(id, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PlusMinus(int id, int quantity)
        {
            var currentItems = await _cartService.GetMyCartItemsAsync();
            var item = currentItems.FirstOrDefault(x => x.Id == id);
            if (item == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            int newQuantity = item.Quantity + quantity;
            if (newQuantity <= 0)
            {
                await _cartService.DeleteItemAsync(id);
                var remain = await _cartService.GetMyCartItemsAsync();
                return Json(new
                {
                    success = true,
                    message = "Đã xoá sản phẩm",
                    cartCount = remain.Sum(x => x.Quantity)
                });
            }
            var (success, message) = await _cartService.UpdateItemAsync(id, newQuantity);
            if (!success)
                return Json(new { success = false, message });
            var updated = await _cartService.GetMyCartItemsAsync();
            return Json(new
            {
                success = true,
                message = "Cập nhật thành công",
                newQuantity,
                cartCount = updated.Sum(x => x.Quantity)
            });
        }


        [HttpGet]
        public IActionResult GetCartBadge()
        {
            return ViewComponent("Cart");
        }

        // Phần CheckOut & PlaceOrder 
        [HttpGet]
        public async Task<IActionResult> CheckOut()
        {
            var apiItems = await _cartService.GetMyCartItemsAsync();
            if (!apiItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index");
            }
            var items = new List<CartItem>();
            foreach (var x in apiItems)
            {
                var variant = await _variantService.GetByIdAsync(x.ProductVariantId);
                var product = variant?.ProductId != null
                    ? await _productService.GetByIdAsync(variant.ProductId.Value)
                    : null;
                items.Add(new CartItem
                {
                    Id = x.Id,
                    CartId = x.CartId,
                    ProductVariantId = x.ProductVariantId,
                    ImageUrl = x.ImageUrl ?? variant?.ImageUrl ?? "",
                    ProductName = product?.ProductName ?? variant?.Name ?? $"Sản phẩm #{x.ProductVariantId}",
                    VariantText = variant?.Name ?? "",
                    Price = x.Price,
                    Quantity = x.Quantity
                });
            }
            return View(items);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            string firstName,
            string lastName,
            string phone,
            string address,
            string country,
            string city,
            string province,
            string paymentMethod,
            string? notes)
        {
            var dto = new OrderCreateDTO
            {
                ShipName = $"{firstName} {lastName}".Trim(),
                ShipPhone = phone,
                ShipAddress = address,
                ShipCountry = country,
                ShipCity = city,
                ShipRegion = province,
                PaymentMethod = paymentMethod,
                Notes = notes
            };
            var (success, message, order) = await _orderService.CreateOrderAsync(dto);
            if (!success || order == null)
            {
                TempData["Error"] = message;
                return RedirectToAction("CheckOut");
            }
            if (paymentMethod == "Chuyển khoản")
            {
                TempData["QrOrder"] = JsonSerializer.Serialize(order);
                return RedirectToAction("QrPayment", "Payment", new { orderId = order.Id });
            }
            return RedirectToAction("Detail", "OrderDetail", new { id = order.Id });
        }
    }
}