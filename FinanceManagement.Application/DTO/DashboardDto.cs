using FinanceManagement.Core.Entities;


namespace FinanceManagement.Application.DTO
{
    public class CategorySummaryDto
    {
        public string CategoryName { get; set; }
        public decimal? Amount { get; set; }
    }

    public class DashboardDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalBalance { get; set; }
        public string CurrencySymbol { get; set; }
        public string BaseCurrencyCode { get; set; }
        public IEnumerable<Transaction> RecentTransaction { get; set; } = new List<Transaction>();

        public List<CategorySummaryDto>? IncomeCategorySummary { get; set; } = new List<CategorySummaryDto>();
        public List<CategorySummaryDto>? ExpenseCategorySummary { get; set; } = new List<CategorySummaryDto>();

    }
}
