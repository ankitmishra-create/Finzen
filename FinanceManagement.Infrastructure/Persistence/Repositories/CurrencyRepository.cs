using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class CurrencyRepository : Repository<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(ApplicationDbContext db) : base(db)
        {

        }

        public async Task<Currency> GetCurrencyAsync(string currencyCode)
        {
            return await _db.Currencies.FirstOrDefaultAsync(c => c.CurrencyCode == currencyCode);

        }

    }
}
