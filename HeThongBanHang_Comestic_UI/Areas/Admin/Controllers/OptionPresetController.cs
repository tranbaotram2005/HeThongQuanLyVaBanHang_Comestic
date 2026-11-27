using HeThongBanHang_Comestic_UI.Areas.Admin.Models;
using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class OptionPresetController : Controller
    {
        private readonly IOptionPresetApiService _presetService;

        public OptionPresetController(IOptionPresetApiService presetService)
        {
            _presetService = presetService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _presetService.GetAllAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            return View(new OptionPresetFormVM { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OptionPresetFormVM model)
        {
            var values = ParseValues(model.ValuesText);
            if (!values.Any())
                ModelState.AddModelError(nameof(model.ValuesText), "Vui lòng nhập ít nhất 1 giá trị mẫu.");

            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _presetService.CreateAsync(new OptionPresetUpsertDTO
            {
                Name = model.Name,
                Values = values,
                Description = model.Description,
                IsActive = model.IsActive
            });

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _presetService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(new OptionPresetFormVM
            {
                Id = item.Id,
                Name = item.Name,
                ValuesText = string.Join(", ", item.Values),
                Description = item.Description,
                IsActive = item.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OptionPresetFormVM model)
        {
            model.Id = id;
            var values = ParseValues(model.ValuesText);
            if (!values.Any())
                ModelState.AddModelError(nameof(model.ValuesText), "Vui lòng nhập ít nhất 1 giá trị mẫu.");

            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _presetService.UpdateAsync(id, new OptionPresetUpsertDTO
            {
                Name = model.Name,
                Values = values,
                Description = model.Description,
                IsActive = model.IsActive
            });

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _presetService.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        private static List<string> ParseValues(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();
            return raw.Split(new[] { ',', '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
