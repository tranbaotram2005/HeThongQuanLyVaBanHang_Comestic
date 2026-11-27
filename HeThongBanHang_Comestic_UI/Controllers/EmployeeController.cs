using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class EmployeeController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Orders", new { area = "Admin" });
        public IActionResult Orders() => RedirectToAction("Index", "Orders", new { area = "Admin" });
        public IActionResult CreateOrder() => RedirectToAction("CreateOrder", "Orders", new { area = "Admin" });
        public IActionResult ApproveOrder(int id, int status = 1, string? returnUrl = null)
            => RedirectToAction("ApproveOrder", "Orders", new { area = "Admin", id, status, returnUrl });
    }
}
