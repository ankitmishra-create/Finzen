using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FinanceManagement.Application.Services
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Guid _defaultCurrencyId = Guid.Parse("b69fabd1-0c5d-42d9-8f98-3b869c0fb631");
        private readonly ILogger<ExternalAuthException> _logger;

        public ExternalAuthService(IUnitOfWork unitOfWork, ILogger<ExternalAuthException> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<User> HandleExternalLoginAsync(AuthenticateResult authResult)
        {
            var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("External login failed: Email claim not provided by external provider.");
                throw new ExternalAuthException("Email claim not provided by external provider.");
            }

            var user = await _unitOfWork.User.GetByEmailAsync(email);
            if (user != null)
            {
                return user;
            }

            var name = authResult.Principal.FindFirstValue(ClaimTypes.Name) ?? "External User";
            var dateOfBirthClaim = authResult.Principal.FindFirstValue(ClaimTypes.DateOfBirth);
            var genderClaim = authResult.Principal.FindFirstValue(ClaimTypes.Gender);

            DateTime.TryParse(dateOfBirthClaim, out DateTime dateOfBirth);

            Enum.TryParse<Gender>(genderClaim, true, out Gender gender);
            if (string.IsNullOrEmpty(genderClaim))
            {
                gender = Gender.NotMentioned;
            }

            var newUser = new User
            {
                FullName = name,
                Email = email,
                IsEmailVerified = true,
                Gender = gender,
                DateOfBirth = dateOfBirth,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = "Admin@123",
                PreferredCurrency = _defaultCurrencyId.ToString(),
                CurrencyId = _defaultCurrencyId
            };

            await _unitOfWork.User.AddUserDataAsync(newUser);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error saving new external user {Email}. {ErrorMessage}", email, ex.InnerException?.Message);
                throw new DatabaseException("Failed to save new user to the database.", ex);
            }

            return newUser;
        }
    }
}

