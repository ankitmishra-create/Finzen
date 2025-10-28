using FinanceManagement.Core.Entities;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManagement.Application.Interfaces
{
    public interface IExternalAuthService
    {
        Task<User> HandleExternalLoginAsync(AuthenticateResult authResult);
    }
}
