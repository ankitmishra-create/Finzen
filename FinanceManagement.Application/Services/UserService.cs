using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHashing _passwordHashing;
        private readonly IGenerateToken _generateToken;
        private readonly ILoggedInUser _loggedInUser;
        private readonly ILogger<UserService> _logger;
        public UserService(ILogger<UserService> logger,IUnitOfWork unitOfWork, IPasswordHashing passwordHashing, IGenerateToken generateToken, ILoggedInUser loggedInUser)
        {
            _unitOfWork = unitOfWork;
            _passwordHashing = passwordHashing;
            _generateToken = generateToken;
            _loggedInUser = loggedInUser;
            _logger = logger;
        }
        public async Task<User?> Register(UserRegistrationVM userRegistrationVM)
        {
            if (await _unitOfWork.User.EmailExistsAsync(userRegistrationVM.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists.", userRegistrationVM.Email);
                throw new DuplicateEmailException("Email Already Exist");
            }
            var passwordHash = _passwordHashing.HashPassword(userRegistrationVM.Password);

            if (!Guid.TryParse(userRegistrationVM.CurrencyId, out var convertedCurrencyId))
            {
                _logger.LogWarning("Registration failed: Invalid CurrencyId format {CurrencyId}", userRegistrationVM.CurrencyId);
                throw new InvalidOperationException("Invalid Currency Id format");
            }

            var newUser = new User()
            {
                FullName = userRegistrationVM.FullName,
                Gender = userRegistrationVM.Gender ?? 0,
                DateOfBirth = userRegistrationVM.DateOfBirth,
                Email = userRegistrationVM.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.Now,
                CurrencyId = convertedCurrencyId,
                PreferredCurrency = userRegistrationVM.CurrencyId.ToString()
            };
            try
            {
                await _generateToken.GenerateTokenAsync(newUser);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("Registration failed: Token Generation cannot access Database");
                throw new DbUpdateException("Database Connection Failed");
            }
            catch(EmailSendException ex)
            {
                _logger.LogError(ex, "Registration failed: Could not send verification email for {Email}", newUser.Email);
                throw new TokenGenerationException("Failed to generate or send email token.", ex);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Registration failed: An unexpected error occurred during token generation for {Email}", newUser.Email);
                throw new TokenGenerationException("An unexpected error occurred during token generation.", ex);
            }
            try
            {
                await _unitOfWork.User.AddUserDataAsync(newUser);
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError(ex, "Registration failed: Database error for {Email}. {ErrorMessage}", newUser.Email, ex.InnerException?.Message);
                throw new DatabaseException("Failed to save new user to the database.", ex);
            }
            return newUser;
        }

        public async Task<LoginResult> Login(UserLoginVM userLoginVM)
        {
            var user = await _unitOfWork.User.GetByEmailAsync(userLoginVM.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for {Email}", userLoginVM.Email);
                return new LoginResult() { Success = false, UserNotFound = true };
            }
            var result = _passwordHashing.VerifyPassword(userLoginVM.Password, user.PasswordHash);
            if (result == false)
            {
                _logger.LogWarning("Login failed: Invalid password for {Email}", userLoginVM.Email);
                return new LoginResult() { Success = false, InvalidPassword = true };
            }
            if (!user.IsEmailVerified)
            {
                _logger.LogInformation("Login failed: Email {Email} is not verified. Re-sending token.", userLoginVM.Email);
                try
                {
                    await _generateToken.GenerateTokenAsync(user);
                }
                catch (TokenGenerationException ex)
                {
                    _logger.LogError(ex, "Login failed: Could not re-send verification email for {Email}", user.Email);
                }
                return new LoginResult() { Success = false, EmailNotVerified = true };
            }
            return new LoginResult() { Success = true, User = user };
        }

        public async Task<AvailableCurrencies> PopulateRegisterationPage()
        {
            try
            {
                var response = await _unitOfWork.Currency.GetAllAsync(c => c == c);
                AvailableCurrencies availableCountries = new AvailableCurrencies();
                availableCountries.currencies = response.ToList();
                return availableCountries;
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Failed to populate registration page: Database error.");
                throw new DatabaseException("Could not retrieve currencies from the database.", ex);
            }
        }

        public async Task<User> GetUser()
        {
            var loggedInUser = _loggedInUser.CurrentLoggedInUser();
            var user = await _unitOfWork.User.GetPopulatedAsync(u => u.UserId == loggedInUser, include: q => q.Include(c => c.Currency));
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }
            return user;
        }
    }
}
