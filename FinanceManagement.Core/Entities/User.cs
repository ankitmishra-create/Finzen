using FinanceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceManagement.Core.Entities
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } 

        [Required]
        public string FullName { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Language { get; set; } = "en";
        public string TimeZone { get; set; } = "Asia/Kolkata";
        public string PreferredCurrency { get; set; } = "Rupees";

        
        public bool IsEmailVerified { get; set; }

        public string? EmailVerificationToken { get; set; }
        public DateTime? VerificationTokenExpiresAt { get; set; }

        
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        
        public void GenerateVerificationToken()
        {
            EmailVerificationToken = Guid.NewGuid().ToString("N");
            VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
            EmailVerificationToken = null;
            VerificationTokenExpiresAt = null;
        }


    }
}
