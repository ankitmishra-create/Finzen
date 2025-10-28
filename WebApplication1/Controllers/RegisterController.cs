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
        public async Task<IActionResult> Index()
        {
            UserRegistrationVM userRegistrationVM = new UserRegistrationVM();
            await PopulateDropdownsAsync();
            
            return View(userRegistrationVM);
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserRegistrationVM userRegistrationVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Gender = new SelectList(Enum.GetValues(typeof(Gender)));
                await PopulateDropdownsAsync();
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
                await PopulateDropdownsAsync();
                return View(userRegistrationVM);
            }
        }

        private async Task PopulateDropdownsAsync()
        {
            var countries = await _userService.PopulateRegisterationPage();
            var countryList = countries.currencies.Select(c => new SelectListItem
            {
                Text = c.CountryName,
                Value = c.CurrencyId.ToString()
            }).ToList();

            ViewBag.Country = countryList;

            var genderList = Enum.GetValues(typeof(Gender))
                         .Cast<Gender>()
                         .Where(g => g != Gender.NotMentioned)
                         .Select(g => new SelectListItem
                         {
                             Text = g.ToString(),
                             Value = g.ToString()
                         })
                         .ToList();
            ViewBag.Gender = new SelectList(genderList, "Value", "Text");
        }
    }
}
