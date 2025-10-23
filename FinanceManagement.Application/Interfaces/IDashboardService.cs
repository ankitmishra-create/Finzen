using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<IEnumerable<Transaction>> Last5Transaction();

        Task<decimal> TotalIncome();
        Task<decimal> TotalExpense();
    }
}
