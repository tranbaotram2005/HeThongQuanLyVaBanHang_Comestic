using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class SupplierController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Supplier", new { area = "Admin" });
        public IActionResult Create() => RedirectToAction("Create", "Supplier", new { area = "Admin" });
        public IActionResult Edit(int id) => RedirectToAction("Edit", "Supplier", new { area = "Admin", id });
    }
}
