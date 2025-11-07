using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using MimeKit.Cryptography;
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

        [Display(Name = "Recurrency Frequency")]
        public Frequency? RecurrenceFrequency { get; set; }


        [Required, Display(Name = "Transaction Timeline")]
        public TransactionTimeLine TransactionTimeLine { get; set; }

        [Display(Name = "Recurring Start Date")]
        public DateTime? RecurringStartDate { get; set; } = DateTime.UtcNow;
        [Display(Name = "Recurring End Date")]
        public DateTime? RecurringEndDate { get; set; } = DateTime.UtcNow.AddYears(10);


        //stepup

        public bool IsStepUpTransaction { get; set; }

        [Display(Name = "Step Up Amount")]
        public decimal? StepUpAmount { get; set; }

        [Display(Name = "Step Up Percentage")]
        public decimal? StepUpPercentage { get; set; }
        [Display(Name = "Step Up Frequency")]
        public Frequency? StepUpFrequeny { get; set; }

        //savings and budgets
        public bool IsForSaving { get; set; }
        public bool IsForBudget { get; set; }

        public List<Saving>? AvailableSavings { get; set; } = new List<Saving>();
        public List<Budget>? AvailableBudgets { get; set; } = new List<Budget>();

        public List<SavingAllocationVm>? SavingAllocationVms { get; set; } = new List<SavingAllocationVm>();
        public List<BudgetAllocationVM>? BudgetAllocationVMs { get; set; } = new List<BudgetAllocationVM>(); 
    }

    public class SavingAllocationVm
    {
        public Guid AllocatedSavigId { get; set; }
        public decimal? Amount { get; set; }
    }

    public class BudgetAllocationVM
    {
        public Guid AllocatedBudgetId { get; set; }
        public decimal? Amount { get; set; }
    }
}
