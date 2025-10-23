using FinanceManagement.Application.Interfaces;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IEmailService _emailService;
        public AccountController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, string token)
        {
            var result = await _emailService.VerifyEmailAsync(userId, token);
            return View("VerifyEmail",result);
        }
    }
}
