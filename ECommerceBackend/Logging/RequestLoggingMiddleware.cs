using System.Diagnostics;
using System.Security.Claims;

namespace ECommerceBackend.Logging
{
    // IMiddleware is an interface in ASP.NET Core used to create custom middleware as a class.
    // in our approach middleware is registered and then the filelogger or database logger works with addscoped lifetime.
    // the middleware instance is created once and reused for the application's lifetime.
    // app.UseMiddleware<RequestLoggingMiddleware>();

    // In IMiddleware approach we dependency inject the IMidlleware and then we register it in program.cs by writing the lifetime
    // we explicitly register the middleware in Program.cs and choose its lifetime in program.cs.
    // builder.Services.AddTransient<RequestLoggingMiddleware>();
    // app.UseMiddleware<RequestLoggingMiddleware>();
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IApplicationLogger logger)
        {
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var startTime = DateTime.Now;

            var stopwatch = Stopwatch.StartNew();

            var correlationId = Guid.TryParse(
                    context.Request.Headers["X-Correlation-ID"].FirstOrDefault(), out var parsedId)
                ? parsedId.ToString()
                : Guid.NewGuid().ToString();

            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            var sessionId = context.Request.Headers["X-Session-Id"].FirstOrDefault();

            if (sessionId?.Length > 100) sessionId = sessionId[..100];

            Exception? exception = null;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // User is only known after authentication has run inside _next
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

                var statusCode = context.Response.StatusCode;

                var controllerName = context.Request.RouteValues["controller"]?.ToString();
                var actionName = context.Request.RouteValues["action"]?.ToString();
                var autoEventName = controllerName != null && actionName != null
                    ? $"{controllerName}.{actionName}"
                    : "";

                var messages = context.Items["LogMessages"] as List<(string Message, string Level)>;

                if (messages == null || messages.Count == 0)
                {
                    messages = new List<(string Message, string Level)>
                    {
                        (
                            exception != null
                                ? $"Unhandled exception during {context.Request.Method} {context.Request.Path}"
                                : statusCode >= 400
                                    ? $"HTTP {context.Request.Method} request to {context.Request.Path} failed with status {statusCode}"
                                    : $"HTTP {context.Request.Method} request to {context.Request.Path} completed",

                            statusCode >= 500 ? "Error"
                                : statusCode >= 400 ? "Warning"
                                : "Information"
                        )
                    };
                }

                try
                {
                    foreach (var message in messages)
                    {
                        var log = new ApplicationLog
                        {
                            Message = message.Message,

                            Level = statusCode >= 500 ? "Error"
                                    : statusCode >= 400 ? "Warning"
                                    : message.Level,

                            EventName = context.Items["EventName"]?.ToString() ?? autoEventName,

                            UserId = userId,
                            EntityId = context.Items["EntityId"]?.ToString(),
                            ErrorCode = context.Items["ErrorCode"]?.ToString(),

                            HttpMethod = context.Request.Method,
                            RequestPath = context.Request.Path,
                            QueryString = context.Request.QueryString.ToString(),
                            RequestStartTime = startTime,
                            ResponseStatusCode = statusCode,
                            ExecutionDuration = stopwatch.ElapsedMilliseconds,
                            ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                            CorrelationId = correlationId,
                            SessionId = sessionId,
                            ExceptionType = "",
                            ExceptionMessage = "",
                            StackTrace = ""
                        };

                        await logger.LogAsync(log);
                    }
                }

                catch (Exception logEx)
                {
                    Console.Error.WriteLine($"Request logging failed: {logEx}");
                    System.Diagnostics.Debug.WriteLine($"Request logging failed: {logEx}");
                }
            }
        }
    }
}