using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<IEnumerable<Transaction>> LastFiveTransaction(Guid userId)
        {
            var result = await _db.Transactions.Where(t => t.UserId == userId).OrderByDescending(u => u.TransactionDate).ThenByDescending(u => u.Amount).Take(5).Include(t => t.Category).ToListAsync();
            return result;
        }

        public async Task<decimal> SumAsync(Expression<Func<Transaction, bool>> predicate)
        {
            return await _db.Transactions.Where(predicate).SumAsync(t => t.Amount) ?? 0;
        }
    }
}
