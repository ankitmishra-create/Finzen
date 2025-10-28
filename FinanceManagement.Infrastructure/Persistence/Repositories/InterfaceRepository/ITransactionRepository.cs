using FinanceManagement.Core.Entities;
using System.Linq.Expressions;

namespace FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> LastFiveTransaction(Guid userId);
        Task<decimal> SumAsync(Expression<Func<Transaction, bool>> predicate);
    }
}
