using FinanceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Application.ViewModels
{
    public class UserRegistrationVM
    {
        [Required(ErrorMessage = "Name is required"),Display(Name ="Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Gender is required"),Display(Name ="Gender")]
        public Gender? Gender { get; set; }

        [Required(ErrorMessage = "Date Of Birth is required"),Display(Name ="Date Of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match"),Display(Name ="Confirm Password")]
        public string ConfirmPassword { get; set; }

        public string CurrencyId { get; set; }
    }
}
