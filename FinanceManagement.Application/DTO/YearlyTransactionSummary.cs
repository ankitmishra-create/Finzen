using FinanceManagement.Core.Enums;

namespace FinanceManagement.Application.DTO
{
    public class YearlyTransactionSummary
    {
        public int Year { get; set; }
        public List<MonthSummary> Months { get; set; } = new List<MonthSummary>();
    }

    public class MonthSummary
    {
        public string MonthName { get; set; }
        public List<CategorySummary> Categories { get; set; } = new List<CategorySummary>();
    }

    public class CategorySummary
    {
        public CategoryType CategoryType { get; set; } 
        public decimal Amount { get; set; }
    }

}
