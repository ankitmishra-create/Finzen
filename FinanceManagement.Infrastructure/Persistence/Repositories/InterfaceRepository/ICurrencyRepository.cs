using FinanceManagement.Core.Entities;

namespace FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository
{
    public interface ICurrencyRepository : IRepository<Currency>
    {
        Task<Currency> GetCurrencyAsync(string currencyCode);
    }
}
