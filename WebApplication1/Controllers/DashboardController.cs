using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            var dashboardData = await _dashboardService.GetDashboardDataAsync();
            SetupCurrencyDropdown(dashboardData.BaseCurrencyCode);
            return View(dashboardData);
        }

        private void SetupCurrencyDropdown(string selectedCurrencyCode)
        {
            var currencySymbols = CurrencySymbol.GetCultures();

            ViewBag.CurrencyList = currencySymbols.Select(c => new SelectListItem
            {
                Value = c.CurrencyCode,
                Text = $"{c.CurrencyName} ({c.CurrencySymbol})",
                Selected = c.CurrencyCode == selectedCurrencyCode
            }).ToList();

            var selectedSymbol = currencySymbols
                .FirstOrDefault(c => c.CurrencyCode == selectedCurrencyCode)?.CurrencySymbol ?? "$";

            ViewBag.CurrencySymbol = selectedSymbol;

            ViewBag.UserCurrencyCode = selectedCurrencyCode;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string selectedCurrencyCode)
        {
           
            var dashboardDto = await _dashboardService.GetDashboardDataAsync();
            var conversionRate = await _dashboardService.CurrencyConversion(selectedCurrencyCode);
            dashboardDto.TotalIncome *= conversionRate;
            dashboardDto.TotalExpense *= conversionRate;
            dashboardDto.TotalBalance *= conversionRate;

            foreach(var transaction in dashboardDto.RecentTransaction)
            {
                if (transaction.Amount.HasValue)
                {
                    transaction.Amount *= conversionRate;
                }
            }

            SetupCurrencyDropdown(selectedCurrencyCode);
            return View(dashboardDto);
        }
    }
}
