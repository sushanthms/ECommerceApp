namespace ECommerceBackend.Logging
{
    public class FileLogger : IApplicationLogger
    {
        private static readonly object _lock = new object();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileLogger(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task LogAsync(ApplicationLog log)
        {
            var logDirectory = "Logs";
            var logFile = Path.Combine(logDirectory, "ecommerce.log");


            Directory.CreateDirectory(logDirectory);

            var logMessage =
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [Information] " +
                $"Message: {log.Message} | " +
                $"HTTP Request | " +
                $"Method: {log.HttpMethod} | " +
                $"Path: {log.RequestPath} | " +
                $"Query: {log.QueryString} | " +
                $"StartTime: {log.RequestStartTime} | " +
                $"StatusCode: {log.ResponseStatusCode} | " +
                $"Duration: {log.ExecutionDuration}ms | " +
                $"ClientIP: {log.ClientIp} | " +
                $"CorrelationId: {log.CorrelationId} | " +
                $"ExceptionType: {log.ExceptionType} | " +
                $"ExceptionMessage: {log.ExceptionMessage} | " +
                $"StackTrace: {log.StackTrace}";

            lock (_lock)
            {
                File.AppendAllText(
                    logFile,
                    logMessage + Environment.NewLine
                );
            }

            return Task.CompletedTask;
        }

        public Task LogMessageAsync(string message)
        {
            _httpContextAccessor.HttpContext!.Items["LogMessage"] = message;

            return Task.CompletedTask;
        }
    }
}