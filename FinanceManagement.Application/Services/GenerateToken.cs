using FinanceManagement.Application.Interfaces;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

namespace FinanceManagement.Infrastructure.Services
{
    public class GenerateToken : IGenerateToken
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public GenerateToken(IUnitOfWork unitOfWork,IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task GenerateTokenAsync(User user)
        {
            if (!user.IsEmailVerified)
            {
                user.GenerateVerificationToken();
            }
            _unitOfWork.User.Update(user);
            await _unitOfWork.SaveAsync();

            string baseUrl = "https://localhost:7145";

            var verifyUrl = $"{baseUrl}/Account/VerifyEmail?userId={Uri.EscapeDataString(user.UserId.ToString())}" +
                            $"&token={Uri.EscapeDataString(user.EmailVerificationToken)}";

            string mailBody = $@"
                <p>Hello,</p>
                <p>Please verify your email by clicking the link below (valid until {user.VerificationTokenExpiresAt:U} UTC):</p>
                <p><a href='{verifyUrl}'>Verify my email</a></p>
                <p>If the link doesn't work, paste this URL into your browser:</p>
                <p>{verifyUrl}</p>";

            await _emailService.SendEmailAsync(user.Email, "Email Verification", mailBody);
        }
    }
}
