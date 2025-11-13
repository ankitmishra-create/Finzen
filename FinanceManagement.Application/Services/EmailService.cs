using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net.Sockets;

namespace FinanceManagement.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailService> _logger;
        public EmailService(IUnitOfWork unitOfWork, ILogger<EmailService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("sandbox.smtp.mailtrap.io", 2525, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("cb9c5770aef454", "b2a4e9ea1506d1");

                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "SMTP Connection Error: Failed to connect to Mailtrap. {ErrorMessage}", ex.Message);
                throw new EmailSendException("Failed to connect to email server.", ex);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "SMTP Authentication Error: Invalid credentials. {ErrorMessage}", ex.Message);
                throw new EmailSendException("Email server authentication failed.", ex);
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "SMTP Command Error: {StatusCode} - {ErrorMessage}", ex.StatusCode, ex.Message);
                throw new EmailSendException($"Failed to send email: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in SendEmailAsync. {ErrorMessage}", ex.Message);
                throw new EmailSendException("An unexpected error occurred while sending the email.", ex);
            }
        }

        public async Task<VerificationResultDto> VerifyEmailAsync(string userId, string token)
        {
            VerificationResultDto verificationResult = new VerificationResultDto();
            string message;
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                verificationResult.Success = false;
                verificationResult.Message = "Missing userId or token.";
                return verificationResult;
            }
            userId = userId.ToLower();
            if (!Guid.TryParse(userId, out Guid uid))
            {
                verificationResult.Success = false;
                verificationResult.Message = "Invalid userId.";
                return verificationResult;
            }
            var user = await _unitOfWork.User.GetByIdAsync(uid);
            if (user == null)
            {
                verificationResult.Success = false;
                verificationResult.Message = "User not found.";
                return verificationResult;
            }
            if (string.IsNullOrWhiteSpace(user.EmailVerificationToken) ||
                !string.Equals(user.EmailVerificationToken, token, StringComparison.Ordinal))
            {
                _logger.LogWarning("VerifyEmailAsync called with missing userId or token.");
                verificationResult.Success = false;
                verificationResult.Message = "Invalid token.";
                return verificationResult;
            }

            if (user.IsEmailVerified)
            {
                verificationResult.Success = true;
                verificationResult.Message = "Email already verified.";
                return verificationResult;
            }
            if (user.VerificationTokenExpiresAt == null || user.VerificationTokenExpiresAt < DateTime.UtcNow)
            {
                verificationResult.Success = false;
                verificationResult.Message = "Token expired.";
                return verificationResult;
            }

            user.VerifyEmail();
            await _unitOfWork.SaveAsync();
            verificationResult.Success = true;
            verificationResult.Message = "Email verified successfully.";
            return verificationResult;
        }
    }
}
