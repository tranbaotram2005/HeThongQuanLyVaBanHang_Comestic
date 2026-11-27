using HeThongBanHang_Comestic_UI.Models;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.ViewComponents
{
    public class MenuCategoryViewComponent : ViewComponent
    {
        private readonly ICategoryApiService _categoryService;
        private readonly IProductApiService _productService;
        public MenuCategoryViewComponent(
            ICategoryApiService categoryService,
            IProductApiService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categoriesTask = _categoryService.GetAllAsync();
            var productsTask = _productService.GetAllAsync();
            await Task.WhenAll(categoriesTask, productsTask);
            var data = categoriesTask.Result.Select(c => new MenuCategory
            {
                categoryId = c.Id,
                categoryName = c.CategoryName,
                soLuong = productsTask.Result.Count(p => p.CategoryId == c.Id)
            }).ToList();
            return View(data);
        }
    }
}
