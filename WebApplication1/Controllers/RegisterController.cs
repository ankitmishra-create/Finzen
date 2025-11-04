using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceManagement.Web.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<RegisterController> _logger;
        public RegisterController(IUserService userService, ILogger<RegisterController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                UserRegistrationVM userRegistrationVM = new UserRegistrationVM();
                await PopulateDropdownsAsync();
                return View(userRegistrationVM);
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Failed to load registration page: Database error.");
                return View("Erorr");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load registration page: Database error.");
                return View("Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> Index(UserRegistrationVM userRegistrationVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Gender = new SelectList(Enum.GetValues(typeof(Gender)));
                    await PopulateDropdownsAsync();
                    return View(userRegistrationVM);
                }
                await _userService.Register(userRegistrationVM);
            }
            catch (DuplicateEmailException ex)
            {
                _logger.LogWarning(ex, "Registration failed: {ErrorMessage}", ex.Message);
                ModelState.AddModelError(nameof(userRegistrationVM.Email), ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed: {ErrorMessage}", ex.Message);
                ModelState.AddModelError(nameof(userRegistrationVM.CurrencyId), ex.Message);
            }
            catch (TokenGenerationException ex)
            {
                _logger.LogError(ex, "Registration failed: Could not generate/send token for {Email}", userRegistrationVM.Email);
                ModelState.AddModelError(string.Empty, "A critical error occurred while creating your account (token failure). Please try again.");
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Registration failed: Database error for {Email}", userRegistrationVM.Email);
                ModelState.AddModelError(string.Empty, "A database error occurred. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed: Unexpected error for {Email}", userRegistrationVM.Email);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            }
            try
            {
                await PopulateDropdownsAsync();
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Failed to re-populate dropdowns after registration error.");
                ModelState.AddModelError(string.Empty, "Error loading page data. Please refresh.");
            }
            return RedirectToAction("Verify", "Login");
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
