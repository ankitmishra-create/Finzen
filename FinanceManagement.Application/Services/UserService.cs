using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;

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
        public async Task<User?> Register(UserRegistrationVM userRegistrationVM)
        {
            if (await (_unitOfWork.User.EmailExistsAsync(userRegistrationVM.Email)))
            {
                return null;
            }
            var passwordHash = _passwordHashing.HashPassword(userRegistrationVM.Password);
            var newUser = new User()
            {
                FullName = userRegistrationVM.FullName,
                Gender = userRegistrationVM.Gender ?? 0,
                DateOfBirth = userRegistrationVM.DateOfBirth,
                Email = userRegistrationVM.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.Now
            };
            await _generateToken.GenerateTokenAsync(newUser);
            await _unitOfWork.User.AddUserAsync(newUser);
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



    }
}
