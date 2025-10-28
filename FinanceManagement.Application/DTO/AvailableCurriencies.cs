using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.DTO
{
    public class AvailableCurrencies
    {
        public List<Currency> currencies { get; set; } = new List<Currency>();
    }
}
