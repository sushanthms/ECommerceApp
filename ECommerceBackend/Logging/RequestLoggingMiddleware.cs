using System.Diagnostics;

namespace ECommerceBackend.Logging
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IApplicationLogger logger)
        {
            var startTime = DateTime.UtcNow;

            var stopwatch = Stopwatch.StartNew();

            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Response.Headers["X-Correlation-ID"] = correlationId;
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
                    HttpMethod = context.Request.Method,
                    RequestPath = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    RequestStartTime = startTime,
                    ResponseStatusCode = context.Response.StatusCode,
                    ExecutionDuration = stopwatch.ElapsedMilliseconds,
                    ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    CorrelationId = correlationId,
                    ExceptionType = exception?.GetType().Name ?? "",
                    ExceptionMessage = exception?.Message ?? "",
                    StackTrace = exception?.StackTrace ?? ""
                };

                await logger.LogAsync(log);
            }
        }
    }
}