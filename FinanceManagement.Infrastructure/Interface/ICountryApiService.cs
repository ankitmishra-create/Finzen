using FinanceManagement.Infrastructure.Persistence.External;

namespace FinanceManagement.Infrastructure.Interface
{
    public interface ICountryApiService
    {
        Task<CountryApiResponesList> GetCountriesAsync();
    }
}
