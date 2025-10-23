using FinanceManagement.Infrastructure.Interface;
using Newtonsoft.Json;

namespace FinanceManagement.Infrastructure.Persistence.External
{
    public class CountryApiService : ICountryApiService
    {
        private readonly HttpClient _httpClient;
        public CountryApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CountryApiResponesList> GetCountriesAsync()
        {
            var url = "https://countries-api-abhishek.vercel.app/countries";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var countryList = JsonConvert.DeserializeObject<CountryApiResponesList>(jsonResponse);
            return countryList;
        }
    }
}
