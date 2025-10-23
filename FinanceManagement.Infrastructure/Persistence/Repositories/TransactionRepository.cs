using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<IEnumerable<Transaction>> LastFiveTransaction(Guid userId)
        {
            var result = await _db.Transactions.Where(t => t.UserId == userId).OrderByDescending(u => u.TransactionDate).ThenByDescending(u => u.Amount).Take(5).ToListAsync();
            return result;
        }

    }
}
