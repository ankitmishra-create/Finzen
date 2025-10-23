using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using FinanceManagement.Core.Entities;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        private readonly ApplicationDbContext _db;
        public TransactionRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Transaction>> LastFiveTransaction(Guid userId)
        {
            var result =await _db.Transactions.Where(t => t.UserId == userId).OrderByDescending(u => u.TransactionDate).Take(5).ToListAsync();
            return result;
        }

    }
}
