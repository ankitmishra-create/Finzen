using FinanceManagement.Application.DTO;
using FinanceManagement.Application.ViewModels;
using FinanceManagement.Core.Entities;
using Microsoft.AspNetCore.Authentication;

namespace FinanceManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> Register(UserRegistrationVM userRegistrationDto);
        Task<LoginResult> Login(UserLoginVM userLoginDto);
        Task<AvailableCurrencies> PopulateRegisterationPage();
        Task<User> GetUser();
    }
}
