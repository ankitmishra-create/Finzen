using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FinanceManagement.Application.ViewModels
{
    public class AddTransactionVM
    {
        public Guid? TransactionId { get; set; }
        public Guid UserId { get; set; }

        public Guid CategoryId { get; set; }

        [NotNull]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Category Type is required")]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public CategoryType CategoryType { get; set; }


        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Transaction Date is required")]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public DateTime TransactionDate { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
