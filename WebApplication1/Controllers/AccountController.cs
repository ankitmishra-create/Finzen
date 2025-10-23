using FinanceManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        public AccountController(IEmailService emailService, IUserService userService)
        {
            _emailService = emailService;
            _userService = userService;
        }

        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, string token)
        {
            var result = await _emailService.VerifyEmailAsync(userId, token);
            return View("VerifyEmail", result);
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl, IsPersistent = true };
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (result.Succeeded == false)
            {
                return View("Error");
            }



            var user = await _userService.ExternalRegistration(result);
            if (user != null)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name,user.FullName.Trim()),
                    new Claim(ClaimTypes.DateOfBirth,user.DateOfBirth.ToString()),
                    new Claim(ClaimTypes.UserData,user.Email.ToString().ToLower()),
                };

                var identiy = new ClaimsIdentity(claims, "MyCookie");
                var principal = new ClaimsPrincipal(identiy);

                var authProps = new AuthenticationProperties()
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(30)
                };
                await HttpContext.SignInAsync(principal, authProps);
                return RedirectToAction("Index", "Dashboard");
            }
            return View("Error");
        }
    }
}
