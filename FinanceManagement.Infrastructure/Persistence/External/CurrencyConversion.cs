using FinanceManagement.Infrastructure.Interface;
using Newtonsoft.Json;

namespace FinanceManagement.Infrastructure.Persistence.External
{
    public class ConvertedRates
    {
        public decimal conversion_rate { get; set; }
    }

    public class CurrencyConversion : ICurrencyConversion
    {
        private readonly HttpClient _httpClient;
        public CurrencyConversion(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ConvertedRates> GetConvertedRates(string baseCurrency, string ConversionCurrency)
        {
            var url = $"https://v6.exchangerate-api.com/v6/7334440275c283f80d1b6c31/pair/{baseCurrency}/{ConversionCurrency}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var convertedRates = JsonConvert.DeserializeObject<ConvertedRates>(jsonResponse);
            return convertedRates;
        }
    }
}
