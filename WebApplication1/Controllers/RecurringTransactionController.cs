using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.Services;
using FinanceManagement.Application.Utility;
using FinanceManagement.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MimeKit.Cryptography;

namespace FinanceManagement.Web.Controllers
{
    public class RecurringTransactionController : Controller
    {

        private readonly IRecurringTransactionService _recurringTransactionService;
        private readonly IDashboardService _dashboardService;
        private readonly IUserService _userService;
        private readonly ITransactionService _transactionService;
        public RecurringTransactionController(ITransactionService transactionService,IUserService userService,IRecurringTransactionService recurringTransactionService, IDashboardService dashboardService)
        {
            _recurringTransactionService = recurringTransactionService;
            _dashboardService = dashboardService;
            _userService = userService;
            _transactionService = transactionService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userService.GetUser();
            var CurrencySymbols = CurrencySymbol.GetCultures();
            foreach (var item in CurrencySymbols)
            {
                if (item.CurrencyCode == user?.Currency?.CurrencyCode)
                {
                    ViewBag.CurrencySymbol = item.CurrencySymbol;
                    break;
                }
            }
            var recurringTransaction = await _recurringTransactionService.GetRecuringTransactionsAsync();
            return View(recurringTransaction);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid recurringTransactionId)
        {
            var recurringTransactionVM = await _recurringTransactionService.EditView(recurringTransactionId);
            var allAvailableCurriencies = CurrencySymbol.GetCultures();
            ViewBag.Available = new SelectList(allAvailableCurriencies, "CurrencyCode", "CurrencyName");
            return View(recurringTransactionVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RecurringTransactionVM recurringTransactionVM)
        {
            await _recurringTransactionService.EditRecurringTransaction(recurringTransactionVM);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid recurringTransactionId)
        {
            await _recurringTransactionService.DeleteRecurringTransaction(recurringTransactionId);
            return RedirectToAction("Index", "RecurringTransaction");
        }

    }
}
