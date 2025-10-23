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
        private readonly ITransactionService _transactionService;
        public RecurringTransactionController(IRecurringTransactionService recurringTransactionService,IDashboardService dashboardService,ITransactionService transactionService)
        {
            _recurringTransactionService = recurringTransactionService;
            _dashboardService = dashboardService;
            _transactionService = transactionService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _dashboardService.GetUser();
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
        public async Task<IActionResult> Edit(Guid transactionId)
        {
            var addTransactionVM = await _transactionService.EditView(transactionId);
            var allavailable = _transactionService.GetAllAvailableCurrency();
            ViewBag.Available = new SelectList(allavailable, "CurrencyCode", "CurrencyName", addTransactionVM.TransactionCurrency);
            return View(addTransactionVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AddTransactionVM addTransactionVM)
        {
            await _transactionService.Edit(addTransactionVM);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid recurringTransactionId)
        {
            await _recurringTransactionService.DeleteRecurringTransaction(recurringTransactionId);
            return RedirectToAction("Index", "RecurringTransaction");
        }

    }
}
