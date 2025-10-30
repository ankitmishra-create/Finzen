using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Web.Controllers
{
    public class TransactionLogController : Controller
    {
        private readonly ITransactionLoggerService _transactionLoggerService;
        private readonly ILogger<TransactionLogController> _logger;
        public TransactionLogController(ITransactionLoggerService transactionLoggerService, ILogger<TransactionLogController> logger)
        {
            _transactionLoggerService = transactionLoggerService;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var deletedTransactions = await _transactionLoggerService.DeletedTransactionLogs();
                return View(deletedTransactions);
            }
            catch (DataRetrievalException ex)
            {
                _logger.LogError(ex, "Failed to load transaction log index: {ErrorMessage}", ex.Message);
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading transaction log index.");
                return View("Error");
            }
        }

        public async Task<IActionResult> Recover(Guid transactionLogId)
        {
            try
            {
                var recoveredTransaction = await _transactionLoggerService.RecoverDeletedTransaction(transactionLogId);
                return RedirectToAction("Index", "Transaction");
            }
            catch (TransactionLogNotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to recover log {LogId}: {ErrorMessage}", transactionLogId, ex.Message);
                return RedirectToAction("Index");
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "Failed to recover log {LogId}: Database error. {ErrorMessage}", transactionLogId, ex.Message);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error recovering log {LogId}", transactionLogId);
                return RedirectToAction("Index");
            }
        }

    }
}
