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
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }

        [Required]
        public Frequency Frequency { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        public DateTime? NextTransactionDate { get; set; }

        public DateTime? LastExecutedDate { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public TransactionTerrority TransactionTerrority { get; set; }

        [Required]
        public string TransactionCurrency { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? OriginalAmount { get; set; }

        public string? OriginalCurrency { get; set; }

        public ICollection<Transaction> GeneratedTransactions { get; set; } = new List<Transaction>();


        public bool? IsStepUpTransaction { get; set; }

        public decimal? StepUpAmount { get; set; }

        public decimal? StepUpPercentage { get; set; }
        public Frequency? StepUpFrequeny { get; set; }
        public DateTime? NextStepUpDate { get; set; }
        public DateTime? LastStepUpDate { get; set; }
    }
}
