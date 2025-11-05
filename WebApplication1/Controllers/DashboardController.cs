using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.Utility;
using FinanceManagement.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceManagement.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;
        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                SetupCurrencyDropdown(dashboardData.BaseCurrencyCode);
                var dashboardGraphData = await _dashboardService.DashboardGraphData();
                DashboardVM dashboardVM = new DashboardVM()
                {
                    DashboardDto = dashboardData,
                    YearlySummary = dashboardGraphData
                };
                return View(dashboardVM);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Dashboard (GET) failed: User not found.");
                return View("Error");
            }
            catch (InvalidCurrencyException ex)
            {
                _logger.LogError(ex, "Dashboard (GET) failed: User has no base currency configured.");
                return View("Error");
            }
            catch (DataRetrievalException ex)
            {
                _logger.LogError(ex, "Dashboard (GET) failed: Database error.");
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Dashboard (GET).");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string selectedCurrencyCode)
        {
            DashboardVM dashboardVM;
            DashboardDto dashboardDto;
            YearlyTransactionSummary dashboardGraphData;
            try
            {
                dashboardDto = await _dashboardService.GetDashboardDataAsync();
                dashboardGraphData = await _dashboardService.DashboardGraphData();
                SetupCurrencyDropdown(selectedCurrencyCode);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard (POST) failed at GetDashboardDataAsync: {ErrorMessage}", ex.Message);
                return View("Error");
            }
            decimal conversionRate = 1.0m;
            try
            {
                if (string.IsNullOrEmpty(selectedCurrencyCode))
                {
                    _logger.LogWarning("Dashboard (POST): No currency code selected.");
                    selectedCurrencyCode = dashboardDto.BaseCurrencyCode;
                }
                conversionRate = await _dashboardService.CurrencyConversion(selectedCurrencyCode);
            }
            catch (CurrencyConversionException ex)
            {
                _logger.LogWarning(ex, "Dashboard (POST): Currency conversion failed. {ErrorMessage}", ex.Message);
                selectedCurrencyCode = dashboardDto.BaseCurrencyCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard (POST): An unexpected error occurred during currency conversion. {ErrorMessage}", ex.Message);
                selectedCurrencyCode = dashboardDto.BaseCurrencyCode;
            }

            dashboardDto.TotalIncome *= conversionRate;
            dashboardDto.TotalExpense *= conversionRate;
            dashboardDto.TotalBalance *= conversionRate;

            foreach (var transaction in dashboardDto.RecentTransaction)
            {
                if (transaction.Amount.HasValue)
                {
                    transaction.Amount *= conversionRate;
                }
            }
            foreach(var month in dashboardGraphData.Months)
            {
                foreach(var category in month.Categories)
                {
                    category.Amount = category.Amount * conversionRate;
                }
            }
            dashboardVM = new DashboardVM()
            {
                DashboardDto = dashboardDto,
                YearlySummary = dashboardGraphData
            };
            return View(dashboardVM);
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

    }
}
