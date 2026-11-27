using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class CategoryController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Category", new { area = "Admin" });
        public IActionResult Create() => RedirectToAction("Create", "Category", new { area = "Admin" });
        public IActionResult Edit(int id) => RedirectToAction("Edit", "Category", new { area = "Admin", id });
    }
}
