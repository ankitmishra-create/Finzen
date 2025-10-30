using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.Interfaces
{
    public interface ITransactionLoggerService
    {
        public Task<List<TransactionLog>> DeletedTransactionLogs();

        public Task<Transaction> RecoverDeletedTransaction(Guid transactionLogId);

    }
}
