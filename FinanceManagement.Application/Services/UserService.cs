using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHashing _passwordHashing;
        private readonly IGenerateToken _generateToken;
        private readonly ILoggedInUser _loggedInUser;
        public UserService(IUnitOfWork unitOfWork, IPasswordHashing passwordHashing, IGenerateToken generateToken, ILoggedInUser loggedInUser)
        {
            _unitOfWork = unitOfWork;
            _passwordHashing = passwordHashing;
            _generateToken = generateToken;
            _loggedInUser = loggedInUser;
        }
        public async Task<User?> Register(UserRegistrationVM userRegistrationVM)
        {
            if (await _unitOfWork.User.EmailExistsAsync(userRegistrationVM.Email))
            {
                throw new DuplicateEmailException("Email Already Exist");
            }
            var passwordHash = _passwordHashing.HashPassword(userRegistrationVM.Password);

            if (!Guid.TryParse(userRegistrationVM.CurrencyId, out var convertedCurrencyId))
            {
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
            await _generateToken.GenerateTokenAsync(newUser);
            await _unitOfWork.User.AddUserDataAsync(newUser);
            return newUser;
        }
        public async Task<LoginResult> Login(UserLoginVM userLoginVM)
        {
            var user = await _unitOfWork.User.GetByEmailAsync(userLoginVM.Email);
            if (user == null)
            {
                return new LoginResult() { Success = false, UserNotFound = true };
            }
            var result = _passwordHashing.VerifyPassword(userLoginVM.Password, user.PasswordHash);
            if (result == false)
            {
                return new LoginResult() { Success = false, InvalidPassword = true };
            }
            if (!user.IsEmailVerified)
            {
                await _generateToken.GenerateTokenAsync(user);
                return new LoginResult() { Success = false, EmailNotVerified = true };
            }
            return new LoginResult() { Success = true, User = user };
        }

        public async Task<AvailableCurrencies> PopulateRegisterationPage()
        {
            var response = await _unitOfWork.Currency.GetAllAsync(c => c == c);
            AvailableCurrencies availableCountries = new AvailableCurrencies();
            availableCountries.currencies = response.ToList();
            return availableCountries;
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
