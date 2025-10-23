using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Persistence.Seeders
{
    public class CountrySeeders
    {
        private readonly ApplicationDbContext _db;
        private readonly ICountryApiService _countryApiService;
        private readonly IUnitOfWork _unitOfWork;

        public CountrySeeders(ApplicationDbContext db, ICountryApiService countryApiService, IUnitOfWork unitOfWork)
        {
            _db = db;
            _countryApiService = countryApiService;
            _unitOfWork = unitOfWork;
        }

        public async Task SeedAsync()
        {
            if (_db.Currencies.Any())
            {
                return;
            }

            var countries = await _countryApiService.GetCountriesAsync();
            if (countries.data.Count == 0)
            {
                return;
            }


            foreach (var country in countries.data)
            {
                Currency currency = new Currency();
                currency.CountryName = country.name;
                currency.CurrencyName = country.currency;

                var currencyCodeSplited = country.currency.Split(" ");
                var currencyCodeWord = currencyCodeSplited[currencyCodeSplited.Count() - 1];
                var currencyCode = currencyCodeWord.Skip(1).SkipLast(1).ToArray();

                currency.CurrencyCode = new string(currencyCode);
                await _db.Currencies.AddAsync(currency);
            }
            await _db.SaveChangesAsync();
            return;
        }

        
    }

}

