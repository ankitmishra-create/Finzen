using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Core.Enums;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace FinanceManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHashing _passwordHashing;
        private readonly IEmailService _emailService;
        private readonly IGenerateToken _generateToken;
        public UserService(IUnitOfWork unitOfWork, IPasswordHashing passwordHashing, IEmailService emailService, IGenerateToken generateToken)
        {
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _passwordHashing = passwordHashing;
            _generateToken = generateToken;
        }
        public async Task<User?> Register(UserRegistrationVM userRegistrationVM,string? Method)
        {
            if (await _unitOfWork.User.EmailExistsAsync(userRegistrationVM.Email))
            {
                return null;
            }
            var passwordHash = _passwordHashing.HashPassword(userRegistrationVM.Password);

            Guid.TryParse(userRegistrationVM.CurrencyId, out var convertedCurrencyId);

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
            if (Method == null)
            {
                await _generateToken.GenerateTokenAsync(newUser);
                await _unitOfWork.User.AddUserDataAsync(newUser);
            }
            else
            {
                await _unitOfWork.User.AddUserDataAsync(newUser);
                await _unitOfWork.SaveAsync();
            }
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

        public async Task<User> ExternalRegistration(AuthenticateResult result)
        {
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var identifier = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var genderClaim = result.Principal.FindFirstValue(ClaimTypes.Gender) ?? "Not Mentioned";
            var dateOfBirthClaim = result.Principal.FindFirstValue(ClaimTypes.Gender) ?? "Not Mentioned";

            if (email == null)
            {
                return null;
            }

            var user = await _unitOfWork.User.GetByEmailAsync(email);
            if (user == null)
            {
                DateTime dateOfBirth = DateTime.Now;
                if (DateTime.TryParse(dateOfBirthClaim, out DateTime convertedDateOfBirth) || dateOfBirthClaim == "Not Mentioned")
                {
                    dateOfBirth = default(DateTime);
                }

                Gender gender = Gender.Male;
                if (Enum.TryParse<Gender>(genderClaim, out Gender convertedGender )|| genderClaim == "Not Mentioned")
                {
                    gender = Gender.NotMentioned;
                }

                UserRegistrationVM userRegistrationVM = new UserRegistrationVM()
                {
                    FullName = name,
                    Email = email,
                    Password = "Admin@123",
                    ConfirmPassword = "Admin@123",
                    DateOfBirth = dateOfBirth,
                    Gender = gender
                };
                var userRegistered = await Register(userRegistrationVM,"ExternalLogin");
                if (userRegistered == null)
                {
                    return null;
                }
                return userRegistered;
            }
            else
            {
                return user;
            }
        }

        public async Task<AvailableCountries> PopulateRegisterationPage()
        {
            var response = await _unitOfWork.Currency.GetAllAsync(c => c==c);
            AvailableCountries availableCountries = new AvailableCountries();
            availableCountries.currencies = response.ToList();
            return availableCountries;
        }


    }
}
