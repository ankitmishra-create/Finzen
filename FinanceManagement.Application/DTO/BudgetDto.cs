using FinanceManagement.Core.Enums;

namespace FinanceManagement.Application.DTO
{
    public class BudgetDto
    {
        public Guid BudgetId { get; set; }
        public CategoryType? CategoryType { get; set; }
        public string BudgetName { get; set; }
        public string? CategoryName { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal AlreadySpendAmount { get; set; }
        public DateTime BudgetStartDate { get; set; }
        public DateTime BudgetEndDate { get; set; }
        public string Description { get; set; }
    }
}
