using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.DTO
{
    public class AvailableCountries
    {
        public List<Currency> currencies { get; set; } = new List<Currency>();
    }
}
