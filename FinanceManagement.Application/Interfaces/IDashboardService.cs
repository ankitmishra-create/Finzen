using FinanceManagement.Application.DTO;
using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
        Task<decimal> CurrencyConversion(string currencyToConvert);
    }

}
