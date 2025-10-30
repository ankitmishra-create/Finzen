using FinanceManagement.Application.DTO;
using FinanceManagement.Application.Exceptions;
using FinanceManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManagement.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        private readonly IExternalAuthService _externalAuthService;
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger, IEmailService emailService, IUserService userService, IExternalAuthService externalAuthService)
        {
            _emailService = emailService;
            _userService = userService;
            _externalAuthService = externalAuthService;
            _logger = logger;
        }

        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, string token)
        {
            try
            {
                var result = await _emailService.VerifyEmailAsync(userId, token);
                return View("VerifyEmail", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during email verification for UserID {UserId}", userId);
                var errorResult = new VerificationResultDto
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later."
                };
                return View("VerifyEmail", errorResult);
            }
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl, IsPersistent = true };
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync("Google");
                if (result.Succeeded == false)
                {
                    _logger.LogWarning("ExternalLoginCallback failed at AuthenticateAsync: {FailureMessage}", result.Failure?.Message);
                    return View("Error");
                }
                var user = await _externalAuthService.HandleExternalLoginAsync(result);
                if (user != null)
                {
                    var claims = new List<Claim>()
                    { 
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Name,user.FullName.Trim()),
                        new Claim(ClaimTypes.DateOfBirth,user.DateOfBirth.ToString()),
                        new Claim(ClaimTypes.UserData,user.Email.ToString().ToLower()),
                    };

                    var identiy = new ClaimsIdentity(claims, "MyCookie");
                    var principal = new ClaimsPrincipal(identiy);

                    var authProps = new AuthenticationProperties()
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    };
                    await HttpContext.SignInAsync(principal, authProps);
                    return RedirectToAction("Index", "Dashboard");
                }
                return View("Error");
            }
            catch (ExternalAuthException ex)
            {
                _logger.LogWarning(ex, "External Authentication Failed: {ErrorMessage}", ex.Message);
                return View("Error");
            }
            catch (DatabaseException ex)
            {
                _logger.LogError(ex, "External Login - Database Failed: {ErrorMessage}", ex.Message);
                return View("Error");
            }
            catch (DuplicateEmailException ex)
            {
                _logger.LogWarning(ex, "External Login - Duplicate Email: {ErrorMessage}", ex.Message);
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in ExternalLoginCallback.");
                return View("Error");
            }
        }
    }
}
