using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Persistence.Repositories;

public class StockTransactionRepository : Repository<StockTransactions> , IStockTransactionRepository
{
    public StockTransactionRepository(ApplicationDbContext context) : base(context)
    {
        
    }
}