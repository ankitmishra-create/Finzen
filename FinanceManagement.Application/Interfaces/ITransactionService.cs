using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Utility;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;

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
        List<CurrencyList> GetAllAvailableCurrency();
    }
}
