using System.Diagnostics;

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
            var startTime = DateTime.Now;

            var stopwatch = Stopwatch.StartNew();

            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            var sessionId = context.Request.Headers["X-Session-Id"].FirstOrDefault();

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

                var log = new ApplicationLog
                {
                    Message = context.Items["LogMessage"]?.ToString() ?? $"HTTP {context.Request.Method} request to {context.Request.Path} completed",
                    Level = exception != null ? "Error" : context.Items["LogLevel"]?.ToString() ?? "Information",
                    HttpMethod = context.Request.Method,
                    RequestPath = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    RequestStartTime = startTime,
                    ResponseStatusCode = context.Response.StatusCode,
                    ExecutionDuration = stopwatch.ElapsedMilliseconds,
                    ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    CorrelationId = correlationId,
                    SessionId = sessionId,
                    ExceptionType = exception?.GetType().Name ?? "",
                    ExceptionMessage = exception?.Message ?? "",
                    StackTrace = exception?.StackTrace ?? ""
                };

                await logger.LogAsync(log);
            }
        }
    }
}