using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class CategoryController : Controller
    {
        private readonly ICategoryApiService _categoryService;
        private readonly IWebHostEnvironment _env;

        public CategoryController(ICategoryApiService categoryService, IWebHostEnvironment env)
        {
            _categoryService = categoryService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _categoryService.GetAllForAdminAsync();
            return View(list);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDTO model, IFormFile? PictureFile)
        {
            if (!ModelState.IsValid) return View(model);

            var uploaded = await SaveImageAsync(PictureFile);
            if (!string.IsNullOrEmpty(uploaded))
                model.Picture = uploaded;

            var success = await _categoryService.CreateAsync(model);
            if (!success)
            {
                ModelState.AddModelError("", "Không thể thêm danh mục.");
                return View(model);
            }

            TempData["Success"] = "Thêm danh mục thành công.";
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
        public async Task<IActionResult> Edit(int id, CategoryUpdateDTO model, IFormFile? PictureFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = id;
                return View(model);
            }

            var uploaded = await SaveImageAsync(PictureFile);
            if (!string.IsNullOrEmpty(uploaded))
                model.Picture = uploaded;

            var success = await _categoryService.UpdateAsync(id, model);
            if (!success)
            {
                ModelState.AddModelError("", "Không thể cập nhật danh mục.");
                ViewBag.CategoryId = id;
                return View(model);
            }

            TempData["Success"] = "Cập nhật danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var success = await _categoryService.UpdateStatusAsync(id, 0);
            TempData[success ? "Success" : "Error"] = success ? "Đã ẩn danh mục." : "Không thể ẩn danh mục.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Show(int id)
        {
            var success = await _categoryService.UpdateStatusAsync(id, 1);
            TempData[success ? "Success" : "Error"] = success ? "Đã hiện lại danh mục." : "Không thể hiện lại danh mục.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            var folder = Path.Combine(_env.WebRootPath, "Category");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }
    }
}
