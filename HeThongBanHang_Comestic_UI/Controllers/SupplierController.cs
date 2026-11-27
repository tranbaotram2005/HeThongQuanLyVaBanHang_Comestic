using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class SupplierController : Controller
    {
        private readonly ISupplierApiService _supplierService;
        private readonly IWebHostEnvironment _env;

        public SupplierController(ISupplierApiService supplierService, IWebHostEnvironment env)
        {
            _supplierService = supplierService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _supplierService.GetAllForAdminAsync();
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierCreateDTO dto, IFormFile? PictureFile)
        {
            if (!ModelState.IsValid) return View(dto);

            var uploadedFileName = await SaveSupplierLogoAsync(PictureFile);
            if (!string.IsNullOrEmpty(uploadedFileName))
                dto.Picture = uploadedFileName;

            var (success, message) = await _supplierService.CreateAsync(dto);
            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(dto);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var s = (await _supplierService.GetAllForAdminAsync()).FirstOrDefault(x => x.Id == id);
            if (s == null) return NotFound();

            ViewBag.SupplierId = id;
            var dto = new SupplierUpdateDTO
            {
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                ContactTitle = s.ContactTitle,
                Phone = s.Phone,
                Email = s.Email,
                HomePage = s.HomePage,
                Notes = s.Notes,
                Picture = s.Picture
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierUpdateDTO dto, IFormFile? PictureFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SupplierId = id;
                return View(dto);
            }

            var uploadedFileName = await SaveSupplierLogoAsync(PictureFile);
            if (!string.IsNullOrEmpty(uploadedFileName))
                dto.Picture = uploadedFileName;

            var (success, message) = await _supplierService.UpdateAsync(id, dto);
            if (!success)
            {
                ModelState.AddModelError("", message);
                ViewBag.SupplierId = id;
                return View(dto);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id, string? returnUrl = null)
        {
            await _supplierService.UpdateStatusAsync(id, 0);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Show(int id, string? returnUrl = null)
        {
            await _supplierService.UpdateStatusAsync(id, 1);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveSupplierLogoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var folder = Path.Combine(_env.WebRootPath, "Supplier");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }
    }
}