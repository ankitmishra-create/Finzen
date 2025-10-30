using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Web.Controllers
{
    [Authorize]
    public class LogoutController : Controller
    {
        private readonly ILogger<LoginController> _logger;

        public LogoutController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Logout()
        {
            string? userName = User?.Identity?.Name ?? "Unknown";
            try
            {
                await HttpContext.SignOutAsync("MyCookie");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error during sign out for user {UserName}.", userName);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
