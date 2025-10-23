using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class CurrencyRepository : Repository<Currency> , ICurrencyRepository
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
