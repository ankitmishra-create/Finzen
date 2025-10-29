using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Application.ViewModels
{
    public class AddTransactionVM
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        [Required, Display(Name = "Category")]
        public Guid? CategoryId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "Category Type is required"), Display(Name = "Category Type")]
        public CategoryType? CategoryType { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Transaction Date is required"), Display(Name = "Transaction Date")]
        public DateTime? TransactionDate { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();


        [Required(ErrorMessage = "Transaction Type is required"), Display(Name = "Transaction Terrority")]
        public TransactionTerrority TransactionTerrority { get; set; }

        [Display(Name = "Transaction Currency")]
        public string? SelectedCurrency { get; set; }

        [Display(Name ="Recurrency Frequency")]
        public RecurrenceFrequency? RecurrenceFrequency { get; set; }

        

        [Required,Display(Name ="Transaction Timeline")] 
        public TransactionTimeLine TransactionTimeLine { get; set; }

        [Display(Name = "Recurring Start Date")]
        public DateTime? RecurringStartDate { get; set; } = DateTime.UtcNow;
        [Display(Name ="Recurring End Date")]
        public DateTime? RecurringEndDate { get; set; } = DateTime.UtcNow.AddYears(10);


        //stepup

        public bool IsStepUpTransaction { get; set; }

        [Display(Name ="Step Up Amount")]
        public decimal? StepUpAmount { get; set; }

        [Display(Name ="Step Up Percentage")]
        public decimal? StepUpPercentage { get; set; }
        [Display(Name ="Step Up Frequency")]
        public RecurrenceFrequency? StepUpFrequeny { get; set; }

    }


}
