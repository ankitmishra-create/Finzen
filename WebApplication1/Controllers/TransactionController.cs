using FinanceManagement.Application.Exceptions;
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
        private readonly ILogger<TransactionController> _logger;
        public TransactionController(ILogger<TransactionController> logger, ITransactionService transactionService, IDashboardService dashboardService, IUserService userService)
        {
            _transactionService = transactionService;
            _dashboardService = dashboardService;
            _userService = userService;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            try
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
            catch (DataRetrievalException ex)
            {
                _logger.LogError(ex, "Failed to display transactions: {ErrorMessage}", ex.Message);
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error displaying transactions.");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var addTransactionVM = await _transactionService.CreateView();
                var allavailableCurriences = _transactionService.GetAllAvailableCurrency();
                ViewBag.Available = new SelectList(allavailableCurriences, "CurrencyCode", "CurrencyName");
                ViewBag.RecurrenceFrequency = new SelectList(Enum.GetValues(typeof(Frequency)));
                return View(addTransactionVM);
            }
            catch (Exception ex) when (ex is UserNotFoundException or InvalidCurrencyException or DataRetrievalException)
            {
                _logger.LogError(ex, "Failed to load Create Transaction view: {ErrorMessage}", ex.Message);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading Create Transaction view.");
                return RedirectToAction("Index");
            }
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
                try
                {
                    await _transactionService.AddTransactionAsync(addTransactionVM);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error loading Create Transaction.");
                    return RedirectToAction("Index");
                }
            }
            try
            {
                addTransactionVM.Categories = (await _transactionService.CreateView()).Categories;
                var allavailable = _transactionService.GetAllAvailableCurrency();
                ViewBag.Available = new SelectList(allavailable, "CurrencyCode", "CurrencyName");
                ViewBag.RecurrenceFrequency = new SelectList(Enum.GetValues(typeof(Frequency)));
                return View(addTransactionVM);
            }
            catch (Exception ex) when (ex is UserNotFoundException or InvalidCurrencyException or DataRetrievalException)
            {
                _logger.LogError(ex, "Failed to load Create Transaction view: {ErrorMessage}", ex.Message);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading Create Transaction view.");
                return RedirectToAction("Index");
            }

        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid transactionId)
        {
            try
            {
                var addTransactionVM = await _transactionService.EditView(transactionId);
                var allavailable = _transactionService.GetAllAvailableCurrency();
                ViewBag.Available = new SelectList(allavailable, "CurrencyCode", "CurrencyName", addTransactionVM.SelectedCurrency);
                return View(addTransactionVM);
            }
            catch (TransactionNotFoundException ex)
            {
                _logger.LogError(ex, "Display Transaction: Transaction Not Found");
                throw new TransactionNotFoundException("Transaction Not Found");
            }
            catch (Exception ex) when (ex is DataRetrievalException or DatabaseException)
            {
                _logger.LogError(ex, "Edit (GET) failed: Database error for {TransactionId}.", transactionId);
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Edit (GET) for {TransactionId}.", transactionId);
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AddTransactionVM addTransactionVM)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(addTransactionVM.SelectedCurrency);
                return View(addTransactionVM);
            }
            try
            {
                await _transactionService.Edit(addTransactionVM);
                return RedirectToAction(nameof(Index));
            }
            catch (TransactionNotFoundException ex)
            {
                _logger.LogError(ex, "Display Transaction: Transaction Not Found");
                throw new TransactionNotFoundException("Transaction Not Found");
            }
            catch (CategoryNotFoundException ex)
            {
                ModelState.AddModelError(nameof(addTransactionVM.CategoryId), ex.Message);
            }
            catch (CurrencyConversionException ex)
            {
                ModelState.AddModelError(nameof(addTransactionVM.SelectedCurrency), ex.Message);
            }

            catch (Exception ex) when (ex is UserNotFoundException or InvalidCurrencyException or DatabaseException)
            {
                _logger.LogError(ex, "Failed to edit transaction: {ErrorMessage}", ex.Message);
                ModelState.AddModelError(string.Empty, $"A critical error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error editing transaction.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            }
            await PopulateDropdownsAsync(addTransactionVM.SelectedCurrency);
            return View(addTransactionVM);
        }

        public async Task<IActionResult> Delete(Guid transactionId)
        {
            try
            {
                await _transactionService.Delete(transactionId);

            }
            catch (TransactionNotFoundException ex)
            {
                _logger.LogError(ex, "Display Transaction: Transaction Not Found");
                throw new TransactionNotFoundException("Transaction Not Found");
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Delete failed: Database error for {TransactionId}.", transactionId);
                TempData["ErrorMessage"] = $"A database error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting {TransactionId}.", transactionId);
                TempData["ErrorMessage"] = "An unexpected error occurred.";
            }
            return RedirectToAction(nameof(Index));
        }

        private Task PopulateDropdownsAsync(string selectedCurrency = null)
        {
            var allavailableCurriences = _transactionService.GetAllAvailableCurrency();
            ViewBag.Available = new SelectList(allavailableCurriences, "CurrencyCode", "CurrencyName", selectedCurrency);
            ViewBag.RecurrenceFrequency = new SelectList(Enum.GetValues(typeof(Frequency)));
            return Task.CompletedTask;
        }

    }
}