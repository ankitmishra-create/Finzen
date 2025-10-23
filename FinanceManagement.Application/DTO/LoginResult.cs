using FinanceManagement.Core.Entities;

namespace FinanceManagement.Application.DTO
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public bool UserNotFound { get; set; }
        public bool EmailNotVerified { get; set; }
        public bool InvalidPassword { get; set; }
        public User? User { get; set; }
    }
}
