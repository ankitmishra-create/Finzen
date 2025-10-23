using FinanceManagement.Core.Entities;

namespace FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> LastFiveTransaction(Guid userId);
    }
}
