using FinanceManagement.Application.DTO;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;

namespace FinanceManagement.Application.Interfaces
{
    public interface ITransactionService
    {
        Task AddTransactionAsync(AddTransactionVM addTransactionVM);
        Task<AddTransactionVM> CreateView();
        Task<IEnumerable<Transaction>> DisplayTransactions();
        Task<AddTransactionVM> EditView(Guid transactionId);
        Task Edit(AddTransactionVM addTransactionVM);
        Task Delete(Guid transactionId);
        List<CurrencyData> GetAllAvailableCurrency();
        void TransactionLog(Transaction transaction, ActionPerformed actionPerformed);
    }
}
