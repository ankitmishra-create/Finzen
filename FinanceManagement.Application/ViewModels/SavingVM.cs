using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Application.ViewModels
{
    public class SavingVM
    {
        public Guid SavingId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public Guid? CategoryId { get; set; }
        public List<Category> UserCategories { get; set; } = new List<Category>();
        public bool CategorySwitch { get; set; }

        public Frequency? Frequency { get; set; }

        [Required(ErrorMessage = "Saving Name is required.")]
        [StringLength(20, ErrorMessage = "Saving Name cannot exceed 20 characters.")]
        public string SavingName { get; set; }

        public bool CustomFrequency { get; set; }
        public DateTime? SavingStartDate { get; set; }
        public DateTime? SavingEndDate { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }

        [Display(Name = "Already Saved Amount")]
        public decimal AlreadySavedAmount { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        public string UserBaseCurrency { get; set; }
    }
}
