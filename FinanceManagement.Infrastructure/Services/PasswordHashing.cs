using FinanceManagement.Application.Interfaces;

namespace FinanceManagement.Application.Services
{
    public class PasswordHashing : IPasswordHashing
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
        }
        public bool VerifyPassword(string password, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedPassword);
        }
    }
}
