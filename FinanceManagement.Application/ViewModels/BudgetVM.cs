using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Application.ViewModels
{
    public class BudgetVM
    {
        [Required]
        public Guid UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public List<Category> UserCategories { get; set; } = new List<Category>();
        public bool CategorySwitch { get; set; }

        public Frequency? Frequency { get; set; }

        [Required(ErrorMessage = "Budget Name is required.")]
        [StringLength(20, ErrorMessage = "Budget Name cannot exceed 20 characters.")]
        public string BudgetName { get; set; }

        public bool CustomFrequency { get; set; }
        public DateTime? BudgetStartDate { get; set; }
        public DateTime? BudgetEndDate { get; set; }

        [Required(ErrorMessage ="Amount is required")]
        public decimal Amount { get; set; }
        
        [Display(Name ="Already Spend Amount")]
        public decimal AlreadySpentAmount { get; set; }

        [Required(ErrorMessage ="Description is required")]
        public string Description { get; set; }

        public string UserBaseCurrency { get; set; }
    }
}
