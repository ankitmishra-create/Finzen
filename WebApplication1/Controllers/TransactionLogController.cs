using FinanceManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Web.Controllers
{
    public class TransactionLogController : Controller
    {
        private readonly ITransactionLoggerService _transactionLoggerService;
        public TransactionLogController(ITransactionLoggerService transactionLoggerService)
        {
            _transactionLoggerService = transactionLoggerService;
        }
        public async Task<IActionResult> Index()
        {
            var deletedTransactions = await _transactionLoggerService.DeletedTransactionLogs();
            return View(deletedTransactions);
        }

        public async Task<IActionResult> Recover(Guid transactionLogId)
        {
            var recoveredTransaction = await _transactionLoggerService.RecoverDeletedTransaction(transactionLogId);
            return RedirectToAction("Index", "Transaction");
        }

    }
}
