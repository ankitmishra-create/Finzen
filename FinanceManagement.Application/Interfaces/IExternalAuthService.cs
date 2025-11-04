using FinanceManagement.Core.Entities;
using Microsoft.AspNetCore.Authentication;

namespace FinanceManagement.Application.Interfaces
{
    public interface IExternalAuthService
    {
        Task<User> HandleExternalLoginAsync(AuthenticateResult authResult);
    }
}
