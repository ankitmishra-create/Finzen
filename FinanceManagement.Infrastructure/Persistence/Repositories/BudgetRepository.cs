using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class BudgetRepository : Repository<Budget> , IBudgetRepository
    {
        public BudgetRepository(ApplicationDbContext db) : base(db)
        {
            
        }
    }
}
