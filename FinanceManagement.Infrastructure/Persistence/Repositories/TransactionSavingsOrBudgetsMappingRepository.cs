using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class TransactionSavingsOrBudgetsMappingRepository : Repository<TransactionSavingsOrBudgetsMapping>, ITransactionSavingsOrBudgetsMappingRepository
    {
        public TransactionSavingsOrBudgetsMappingRepository(ApplicationDbContext db) : base(db)
        {

        }
    }

}
