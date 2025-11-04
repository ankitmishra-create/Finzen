using FinanceManagement.Application.DTO;

namespace FinanceManagement.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
        Task<decimal> CurrencyConversion(string currencyToConvert);
    }

}
