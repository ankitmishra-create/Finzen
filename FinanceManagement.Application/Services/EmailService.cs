using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using MailKit.Security;
using MimeKit;

namespace FinanceManagement.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork;
        public EmailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Ankit's App", "ankitm7972@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage,
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("sandbox.smtp.mailtrap.io", 2525, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("e8f914e04144e4", "cb476df26d1599");

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public async Task<VerificationResultDto> VerifyEmailAsync(string userId, string token)
        {
            VerificationResultDto verificationResult = new VerificationResultDto();
            string message;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                message = "Missing userId or token.";
                verificationResult.Success = false;
                verificationResult.Message = message;
                return verificationResult;
            }
            userId = userId.ToLower();
            if (!Guid.TryParse(userId, out Guid uid))
            {
                message = "Invalid userId.";
                verificationResult.Success = false;
                verificationResult.Message = message;
                return verificationResult;
            }

            var user = await _unitOfWork.User.GetByIdAsync(uid);
            if (user == null)
            {
                message = "User not found.";
                verificationResult.Success = false;
                verificationResult.Message = message;
                return verificationResult;
            }

            if (string.IsNullOrWhiteSpace(user.EmailVerificationToken) ||
                !string.Equals(user.EmailVerificationToken, token, StringComparison.Ordinal))
            {
                message = "Invalid token.";
                verificationResult.Success = false;
                verificationResult.Message = message;
                return verificationResult;
            }

            if (user.IsEmailVerified)
            {
                message = "Email already verified.";
                verificationResult.Success = true;
                verificationResult.Message = message;
                return verificationResult;
            }
            if (user.VerificationTokenExpiresAt == null || user.VerificationTokenExpiresAt < DateTime.UtcNow)
            {
                message = "Token expired.";
                verificationResult.Success = false;
                verificationResult.Message = message;
                return verificationResult;
            }

            user.VerifyEmail();
            await _unitOfWork.SaveAsync();
            message = "Email verified successfully.";
            verificationResult.Success = true;
            verificationResult.Message = message;
            return verificationResult;
        }
    }
}
