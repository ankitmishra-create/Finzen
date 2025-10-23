using FinanceManagement.Infrastructure.Persistence.External;

namespace FinanceManagement.Infrastructure.Interface
{
    public interface ICurrencyConversion
    {
        Task<ConvertedRates> GetConvertedRates(string baseCurrency, string ConversionCurrency);
    }
}
