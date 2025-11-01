using FinanceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Application.ViewModels
{
    public class RecurringTransactionVM
    {
        public Guid RecurringTransactionId { get; set; }

        [Display(Name = "Recurrency Frequency")]
        public Frequency RecurrenceFrequency { get; set; }
        public string Description { get; set; }

        [Display(Name = "Original Amount")]
        public decimal OriginalAmount { get; set; }

        [Display(Name = "Converted Amount")]
        public decimal ConvertedAmount { get; set; }
        [Display(Name = "Original Currency")]
        public string OriginalCurrency { get; set; }
        [Display(Name = "Transaction Currency")]
        public string TransactionCurrency { get; set; }
        [Display(Name = "Transaction Terrority")]
        public TransactionTerrority TransactionTerrority { get; set; }


        public bool IsStepUpTransaction { get; set; }

        [Display(Name = "Step Up Amount")]
        public decimal? StepUpAmount { get; set; }

        [Display(Name = "Step Up Percentage")]
        public decimal? StepUpPercentage { get; set; }
        [Display(Name = "Step Up Frequency")]
        public Frequency? StepUpFrequeny { get; set; }

    }
}
