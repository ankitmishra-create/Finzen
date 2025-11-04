using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class RecurringTransactionRepository : Repository<RecurringTransactions>, IRecurringTransactionRepository
    {
        public RecurringTransactionRepository(ApplicationDbContext db) : base(db)
        {

        }
    }
}
