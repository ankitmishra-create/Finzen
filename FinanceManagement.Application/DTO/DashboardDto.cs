using FinanceManagement.Core.Entities;


namespace FinanceManagement.Application.DTO
{
    public class DashboardDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalBalance { get; set; }
        public string CurrencySymbol { get; set; }
        public string BaseCurrencyCode { get; set; }
        public IEnumerable<Transaction> RecentTransaction { get; set; } = new List<Transaction>();
    }
}
