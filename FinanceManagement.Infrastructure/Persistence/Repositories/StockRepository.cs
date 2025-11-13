using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Persistence.Repositories;

public class StockRepository : Repository<StockHoldings>,IStocksRepository
{
    public StockRepository(ApplicationDbContext context) : base(context)
    {
        
    }
}