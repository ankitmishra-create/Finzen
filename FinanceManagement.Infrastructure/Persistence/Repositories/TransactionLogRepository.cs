using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class TransactionLogRepository : Repository<TransactionLog>, ITransactionLogRepository
    {
        public TransactionLogRepository(ApplicationDbContext db) : base(db)
        {

        }
    }
}
