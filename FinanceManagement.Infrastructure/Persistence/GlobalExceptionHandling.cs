using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FinanceManagement.Infrastructure.Persistence
{
    public class GlobalExceptionHandling
    {
        private readonly ILogger<GlobalExceptionHandling> _logger;
        private readonly RequestDelegate _next;
        public GlobalExceptionHandling(ILogger<GlobalExceptionHandling> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                _logger.LogError("Unhandled Exception Occured");
                var response = new
                {
                    TimeStamp = DateTime.Now,
                    StatusCode = context.Response.StatusCode,
                    Message = "An Unexpected Error Occured",
                    Source = e.Source,
                    ErrorMessage = e.Message,
                    InnerException = e?.InnerException,
                    StackTrace = e?.StackTrace,
                };
                var json = JsonSerializer.Serialize(response);
                _logger.LogError(json);
            }
        }

    }
}
