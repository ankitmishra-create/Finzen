using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public async Task<IActionResult> Index()
        {
            var last5 = await _dashboardService.Last5Transaction();
            var income = await _dashboardService.TotalIncome();
            var expense = await _dashboardService.TotalExpense();
            var available = income - expense;
            var user = await _dashboardService.GetUser();

            var CurrencySymbols = CurrencySymbol.GetCultures();
            string userCurrencyCode = user?.Currency?.CurrencyCode;

            foreach (var item in CurrencySymbols)
            {
                if (item.CurrencyCode == userCurrencyCode)
                {
                    ViewBag.CurrencySymbol = item.CurrencySymbol;
                    break;
                }
            }

            ViewBag.UserCurrencyCode = userCurrencyCode;
            ViewBag.CurrencyList = CurrencySymbols;
            ViewBag.Income = income;
            ViewBag.Expense = expense;
            ViewBag.Available = available;
            return View(last5);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string selectedCurrencyCode)
        {
            var updatedResult = await _dashboardService.CurrencyConversion(selectedCurrencyCode);
            var last5 = await _dashboardService.Last5Transaction();
            var alltranactions = await _dashboardService.After5();
            foreach (var last in last5)
            {
                last.Amount = last.Amount * updatedResult;
            }
            foreach(var last in alltranactions)
            {
                last.Amount = last.Amount * updatedResult;
            }

            var income = await (_dashboardService.TotalIncome());
            var expense = await (_dashboardService.TotalExpense());
            var available = (income - expense);
            var user = await _dashboardService.GetUser();

            var CurrencySymbols = CurrencySymbol.GetCultures();
            string userCurrencyCode = selectedCurrencyCode;

            foreach (var item in CurrencySymbols)
            {
                if (item.CurrencyCode == userCurrencyCode)
                {
                    ViewBag.CurrencySymbol = item.CurrencySymbol;
                    break;
                }
            }

            ViewBag.UserCurrencyCode = userCurrencyCode;
            ViewBag.CurrencyList = CurrencySymbols;
            ViewBag.Income = income;
            ViewBag.Expense = expense;
            ViewBag.Available = available;
            return View(last5);
        }
    }
}
