using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceManagement.Core.Entities
{
    public class TransactionSavingsOrBudgetsMapping
    {
        public Guid TransactionSavingsOrBudgetsMappingId { get; set; } = Guid.NewGuid();

        public Guid? SavingId { get; set; }
        [ForeignKey("SavingId")]
        public Saving? Saving { get; set; }

        public Guid TransactionId { get; set; }
        [ForeignKey("TransactionId")]
        public Transaction Transaction { get; set; }

        public Guid? BudgetId { get; set; }
        [ForeignKey("BudgetId")]
        public Budget Budget { get; set; }

        public decimal? SavedAmount { get; set; }
    }
}
