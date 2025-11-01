using FinanceManagement.Core.Enums;

namespace FinanceManagement.Core.Entities
{
    public class Budget
    {
        public Guid BudgetId { get; set; } = Guid.NewGuid();

        public string BudgetName { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }

        public Frequency? FrequencyOfBudget { get; set; }

        public decimal BudgetAmount { get; set; }
    
        public bool? CustomBudget { get; set; }
        public DateTime BudgetStartDate { get; set; } = DateTime.UtcNow;
        public DateTime BudgetEndDate { get; set; }

        public string Description { get; set; }

        public decimal AlreadySpendAmount { get; set; }

    
    }

}
