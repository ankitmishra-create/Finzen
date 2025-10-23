using FinanceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FinanceManagement.Application.ViewModels
{
    public class AddCategoryVM
    {
        public Guid Id { get; set; }
        [AllowNull]
        public string? UserId { get; set; } = null;

        [Required, MaxLength(100),Display(Name ="Category Name")]
        public string? CategoryName { get; set; }

        [Required]
        public CategoryType CategoryType { get; set; }

        public SubType? SubType { get; set; } = null;
    }
}
