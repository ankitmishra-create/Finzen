using FinanceManagement.Infrastructure.Persistence.External;

namespace FinanceManagement.Infrastructure.Interface
{
    public interface ICurrencyConversionService
    {
        Task<ConvertedRates> GetConvertedRates(string baseCurrency, string ConversionCurrency);
    }
}
