using HeThongBanHang_Comestic_API.DTOs;
using HeThongBanHang_Comestic_API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang_Comestic_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // POST: api/Account/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _accountService.LoginAsync(model);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Username, Email hoặc mật khẩu không chính xác."
                });
            }

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                data = result
            });
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool result = await _accountService.RegisterAsync(model);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Username hoặc Email đã tồn tại, hoặc Role Customer chưa được cấu hình."
                });
            }

            return Ok(new
            {
                message = "Đăng ký tài khoản thành công!"
            });
        }

        [AllowAnonymous]
        [HttpPost("register-send-otp")]
        public async Task<IActionResult> SendRegisterOtp(RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _accountService.SendRegisterOtpAsync(model);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [AllowAnonymous]
        [HttpPost("register-resend-otp")]
        public async Task<IActionResult> ResendRegisterOtp(ResendRegisterOtpDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _accountService.ResendRegisterOtpAsync(model.Email);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [AllowAnonymous]
        [HttpPost("register-verify-otp")]
        public async Task<IActionResult> VerifyRegisterOtp(VerifyRegisterOtpDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _accountService.VerifyRegisterOtpAsync(model);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

    }
}
