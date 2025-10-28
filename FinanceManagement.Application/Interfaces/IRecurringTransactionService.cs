using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.Interfaces
{
    public interface IRecurringTransactionService
    {
        Task<List<RecurringTransactions>> GetRecuringTransactionsAsync();
        Task DeleteRecurringTransaction(Guid recurringTransactionId);
        Task<RecurringTransactions> CreateRecurringTransaction(AddTransactionVM addTransactionVM, decimal originalAmount);
        Task<RecurringTransactions> EditRecurringTransaction(RecurringTransactionVM recurringTransactionVM);
        Task<RecurringTransactionVM> EditView(Guid recurringTransactionId);
    }
}
