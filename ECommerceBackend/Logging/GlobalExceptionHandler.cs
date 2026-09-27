using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerceBackend.Logging
{// GlobalExceptionHandler uses IApplicationLogger to write the logs, but GlobalExceptionHandler is Singleton by default because it is inherited from IExcpetionHandler
    // GlobalExceptionHandler needs IApplicationLogger, IApplicationLogger is AddScoped, but GlobalExceptionHandler is Singleton, so that is not allowed
    // IServiceScopeFactory allows a Singleton scope to create a temporary scope.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public GlobalExceptionHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString()
                ?? Guid.NewGuid().ToString();

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

            var controllerName = context.Request.RouteValues["controller"]?.ToString();
            var actionName = context.Request.RouteValues["action"]?.ToString();
            var eventName = controllerName != null && actionName != null
                ? $"{controllerName}.{actionName}"
                : "UnhandledException";

            var log = new ApplicationLog
            {
                EventName = eventName,
                UserId = userId,
                ErrorCode = "INTERNAL_SERVER_ERROR",
                Message = "Unhandled exception occurred",
                Level = "Error",
                HttpMethod = context.Request.Method,
                RequestPath = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                RequestStartTime = DateTime.Now,
                ResponseStatusCode = StatusCodes.Status500InternalServerError,
                ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                CorrelationId = correlationId,
                SessionId = context.Request.Headers["X-Session-Id"].FirstOrDefault(),
                ExceptionType = exception.GetType().Name,
                ExceptionMessage = exception.Message,
                StackTrace = exception.StackTrace ?? ""
            };

            // creating a temporary scope, IServiceScopeFactory allows to create a temporaray scope inside Singleton Scope.
            using (var scope = _scopeFactory.CreateScope())
            {
                // using the temporary scope we create the instance of FileLogger or DatabaseLogger
                var logger = scope.ServiceProvider.GetRequiredService<IApplicationLogger>();
                await logger.LogAsync(log);
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Something went wrong. Please try again.",
                correlationId = correlationId
            });

            return true;
        }
    }
}