using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class CategoryController : Controller
    {
        private readonly ICategoryApiService _categoryService;

        public CategoryController(ICategoryApiService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _categoryService.GetAllForAdminAsync();
            return View(list);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDTO model)
        {
            if (!ModelState.IsValid) return View(model);
            await _categoryService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = (await _categoryService.GetAllForAdminAsync()).FirstOrDefault(c => c.Id == id);
            if (item == null) return NotFound();

            ViewBag.CategoryId = id;
            return View(new CategoryUpdateDTO
            {
                CategoryName = item.CategoryName,
                Description = item.Description,
                Picture = item.Picture
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = id;
                return View(model);
            }
            await _categoryService.UpdateAsync(id, model);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id, string? returnUrl = null)
        {
            await _categoryService.UpdateStatusAsync(id, 0);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Show(int id, string? returnUrl = null)
        {
            await _categoryService.UpdateStatusAsync(id, 1);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }
    }
}