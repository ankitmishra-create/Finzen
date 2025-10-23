using FinanceManagement.Application.DTO;

namespace FinanceManagement.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task<VerificationResultDto> VerifyEmailAsync(string userId, string token);
    }
}
