using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManagement.Infrastructure.Persistence.Repositories
{
    public class RecurringTransactionRepository : Repository<RecurringTransactions> ,IRecurringTransactionRepository 
    {
        public RecurringTransactionRepository(ApplicationDbContext db):base(db)
        {
            
        }
    }
}
