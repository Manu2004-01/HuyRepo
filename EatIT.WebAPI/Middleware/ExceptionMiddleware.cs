using EatIT.WebAPI.Errors;
using System.Net;
using System.Text.Json;

namespace EatIT.WebAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment hostEnvironment)
        {
            _next = next;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public async Task InvokeAsync(HttpContext context) 
        {
            try
            {
                await _next(context);
                _logger.LogInformation("Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"This Error come from Exception Middleware {ex.Message}");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var payload = JsonSerializer.Serialize(new BaseCommentResponse(context.Response.StatusCode));
                await context.Response.WriteAsync(payload);

            }
        }
    }
}
