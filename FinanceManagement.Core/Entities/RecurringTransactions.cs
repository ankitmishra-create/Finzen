using FinanceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceManagement.Core.Entities
{
    public class RecurringTransactions
    {
        [Key]
        public Guid RecurringTransactionId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TransactionId { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        public RecurrenceFrequency Frequency { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public DateTime NextTransactionDate { get; set; }

        public DateTime? LastExecutedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
