using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.Utility;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceManagement.Web.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IDashboardService _dashboardService;
        private readonly IUserService _userService;
        public TransactionController(ITransactionService transactionService, IDashboardService dashboardService, IUserService userService)
        {
            _transactionService = transactionService;
            _dashboardService = dashboardService;
            _userService = userService;
        }
        public async Task<IActionResult> Index()
        {
            var allTransaction = await _transactionService.DisplayTransactions();
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
            return View(allTransaction);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var addTransactionVM = await _transactionService.CreateView();
            var allavailableCurriences = _transactionService.GetAllAvailableCurrency();
            ViewBag.Available = new SelectList(allavailableCurriences, "CurrencyCode", "CurrencyName");
            ViewBag.RecurrenceFrequency = new SelectList(Enum.GetValues(typeof(RecurrenceFrequency)));
            return View(addTransactionVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddTransactionVM addTransactionVM)
        {
            if (addTransactionVM.CategoryType == CategoryType.Expense && string.IsNullOrWhiteSpace(addTransactionVM.Description))
            {
                ModelState.AddModelError(nameof(addTransactionVM.Description), "A description is required for all expense transactions.");
            }

            if (addTransactionVM.TransactionTerrority == TransactionTerrority.International && string.IsNullOrWhiteSpace(addTransactionVM.SelectedCurrency))
            {
                ModelState.AddModelError(nameof(addTransactionVM.SelectedCurrency), "Transaction Currency is required for international transactions.");
            }

            if (ModelState.IsValid)
            {
                await _transactionService.AddTransactionAsync(addTransactionVM);
                return RedirectToAction("Index");
            }

            addTransactionVM.Categories = (await _transactionService.CreateView()).Categories;
            var allavailable = _transactionService.GetAllAvailableCurrency();
            ViewBag.Available = new SelectList(allavailable, "CurrencyCode", "CurrencyName");
            ViewBag.RecurrenceFrequency = new SelectList(Enum.GetValues(typeof(RecurrenceFrequency)));
            return View(addTransactionVM);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid transactionId)
        {
            var addTransactionVM = await _transactionService.EditView(transactionId);
            var allavailable = _transactionService.GetAllAvailableCurrency();
            ViewBag.Available = new SelectList(allavailable, "CurrencyCode", "CurrencyName", addTransactionVM.SelectedCurrency);
            return View(addTransactionVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AddTransactionVM addTransactionVM)
        {
            await _transactionService.Edit(addTransactionVM);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid transactionId)
        {
            await _transactionService.Delete(transactionId);
            return RedirectToAction(nameof(Index));
        }

    }
}
