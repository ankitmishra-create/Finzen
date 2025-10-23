namespace FinanceManagement.Infrastructure.Persistence.External
{
    public class CountryApiResponse
    {
        public string name { get; set; }
        public string currency { get; set; }
    }

    public class CountryApiResponesList
    {
        public List<CountryApiResponse> data { get; set; }
    }

    public class ExchangeRates
    {
        public Dictionary<string, decimal> conversion_rates { get; set; }
    }
}
