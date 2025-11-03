using FinanceManagement.Core.Enums;

namespace FinanceManagement.Core.Entities
{
    public class Saving
    {
        public Guid SavingId { get; set; } = Guid.NewGuid();

        public string SavingName { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }

        public Frequency? FrequencyOfSaving { get; set; }

        public decimal SavingAmount { get; set; }

        public bool? CustomSaving { get; set; }
        public DateTime SavingStartDate { get; set; } = DateTime.UtcNow;
        public DateTime SavingEndDate { get; set; }

        public string Description { get; set; }

        public decimal AlreadySavedAmount { get; set; }
    }
}
