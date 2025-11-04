using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.External.Exceptions;
using Newtonsoft.Json;

namespace FinanceManagement.Infrastructure.Persistence.External
{
    public class ConvertedRates
    {
        public decimal conversion_rate { get; set; }
    }

    public class CurrencyConversionService : ICurrencyConversionService
    {
        private readonly HttpClient _httpClient;
        public CurrencyConversionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ConvertedRates> GetConvertedRates(string baseCurrency, string ConversionCurrency)
        {
            var url = $"https://v6.exchangerate-api.com/v6/fa2a2e5b51cb3851a58206c1/pair/{baseCurrency}/{ConversionCurrency}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseNotFoundException($"The api had error returning the response. The Status Code returned is {response.StatusCode}");
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var convertedRates = JsonConvert.DeserializeObject<ConvertedRates>(jsonResponse);
            return convertedRates;

        }
    }
}
