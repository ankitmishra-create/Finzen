using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class SavingRepository : Repository<Saving>, ISavingRepository
    {
        public SavingRepository(ApplicationDbContext db) : base(db)
        {

        }
    }
}
