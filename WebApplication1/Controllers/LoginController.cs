using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            UserLoginVM userLoginVM = new UserLoginVM();
            return View(userLoginVM);
        }

        public IActionResult Verify()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserLoginVM userLoginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(userLoginVM);
            }

            var result = await _userService.Login(userLoginVM);
            if (!result.Success && result.UserNotFound || result.InvalidPassword)
            {
                ModelState.AddModelError(string.Empty, "Enter Valid UserName or password");
                return View(userLoginVM);
            }

            if (!result.Success && result.EmailNotVerified)
            {
                return RedirectToAction("Verify");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.User.UserId.ToString()),
                new Claim(ClaimTypes.Name,result.User.FullName.Trim()),
                new Claim(ClaimTypes.DateOfBirth,result.User.DateOfBirth.ToString()),
                new Claim(ClaimTypes.UserData,result.User.Email.ToString().ToLower())
            };

            var identity = new ClaimsIdentity(claims, "MyCookie");

            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(30),
            };

            await HttpContext.SignInAsync("MyCookie", principal, authProps);
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
