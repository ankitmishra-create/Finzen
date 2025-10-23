using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceManagement.Web.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IUserService _userService;
        public RegisterController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            UserRegistrationVM userRegistrationVM = new UserRegistrationVM();
            ViewBag.Gender = new SelectList(Enum.GetValues(typeof(Gender)));
            return View(userRegistrationVM);
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserRegistrationVM userRegistrationVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Gender = new SelectList(Enum.GetValues(typeof(Gender)));
                return View(userRegistrationVM);
            }
            var result = await _userService.Register(userRegistrationVM);
            if (result != null)
            {
                return RedirectToAction("Verify", "Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User already Exists");
                return View(userRegistrationVM);
            }
        }

    }
}
