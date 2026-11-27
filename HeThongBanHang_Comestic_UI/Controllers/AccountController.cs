using System.Security.Claims;
using HeThongBanHang_Comestic_UI.DTOs;
using HeThongBanHang_Comestic_UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountApiService _accountService;

        public AccountController(IAccountApiService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _accountService.RegisterAsync(model);
            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (!ModelState.IsValid)
                return View(model);

            var (success, message, data) = await _accountService.LoginAsync(model);
            if (!success || data == null)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            // 1. Lưu JWT Token vào Session để dùng cho các Service gọi API sau này
            HttpContext.Session.SetString("AccessToken", data.AccessToken);

            // 2. Tạo Claims để lưu Cookie đăng nhập cho MVC UI
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, data.UserId.ToString()),
                new Claim(ClaimTypes.Name, data.FullName),
                new Claim(ClaimTypes.Email, data.Email),
                new Claim("AccessToken", data.AccessToken)
            };

            if (data.Roles.Any())
            {
                foreach (var role in data.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "Customer"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return Redirect(ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Redirect("/404");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            HttpContext.Session.Remove("AccessToken");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}