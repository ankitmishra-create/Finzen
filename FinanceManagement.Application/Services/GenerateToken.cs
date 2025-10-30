using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Infrastructure.Services
{
    public class GenerateToken : IGenerateToken
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<GenerateToken> _logger;
        public GenerateToken(ILogger<GenerateToken> logger,IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }
        public async Task GenerateTokenAsync(User user)
        {
            if (!user.IsEmailVerified)
            {
                user.GenerateVerificationToken();
            }
            _unitOfWork.User.Update(user);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError("Generate Token: Exception Occured while generating token for {User}", user);
                throw;
            }

            string baseUrl = "https://localhost:7145";

            var verifyUrl = $"{baseUrl}/Account/VerifyEmail?userId={Uri.EscapeDataString(user.UserId.ToString())}" +
                            $"&token={Uri.EscapeDataString(user.EmailVerificationToken)}";

            string mailBody = $@"
                <p>Hello,</p>
                <p>Please verify your email by clicking the link below (valid until {user.VerificationTokenExpiresAt:U} UTC):</p>
                <p><a href='{verifyUrl}'>Verify my email</a></p>
                <p>If the link doesn't work, paste this URL into your browser:</p>
                <p>{verifyUrl}</p>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, "Email Verification", mailBody);
            }
            catch(EmailSendException ex)
            {
                _logger.LogError(ex, "Registration failed: Could not send verification email for {Email}", user.Email);
                throw new TokenGenerationException("Failed to generate or send email token.", ex);
            }
        }
    }
}
