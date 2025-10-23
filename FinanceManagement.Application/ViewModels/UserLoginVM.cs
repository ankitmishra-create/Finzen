using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Application.ViewModels
{
    public class UserLoginVM
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
