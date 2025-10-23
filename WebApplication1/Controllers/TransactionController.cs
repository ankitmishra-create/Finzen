using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Web.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var addTransactionVM = await _transactionService.CreateView();
            return View(addTransactionVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddTransactionVM addTransactionVM)
        {
            if (ModelState.IsValid)
            {
                await _transactionService.AddTransactionAsync(addTransactionVM);
                return RedirectToAction("Display");
            }
            else
            {
                ModelState.AddModelError("Error", "Enter all the required fields");
                addTransactionVM.Categories = _transactionService.CreateView().Result.Categories;
                return View(addTransactionVM);
            }
        }

        public async Task<IActionResult> Display()
        {
            var allTransaction = await _transactionService.DisplayTransactions();
            return View(allTransaction);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid transactionId)
        {
            var addTransactionVM = await _transactionService.EditView(transactionId);
            return View(addTransactionVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AddTransactionVM addTransactionVM)
        {
            await _transactionService.Edit(addTransactionVM);
            return RedirectToAction(nameof(Display));
        }

        public async Task<IActionResult> Delete(Guid transactionId)
        {
            await _transactionService.Delete(transactionId);
            return RedirectToAction(nameof(Display));
        }

    }
}
