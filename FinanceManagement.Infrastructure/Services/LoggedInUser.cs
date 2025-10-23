using FinanceManagement.Infrastructure.Interface;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FinanceManagement.Infrastructure.Services
{
    public class LoggedInUser : ILoggedInUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoggedInUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid CurrentLoggedInUser()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userId, out var userIdresutl);
            return userIdresutl;
        }
    }
}
