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
            var logFileName = $"ecommerce-{DateTime.Now:yyyy-MM-dd}.log";
            var logFile = Path.Combine(logDirectory, logFileName);
            // File.AppendAllText(logFile, logMessage + Environment.NewLine); creates the file

            Directory.CreateDirectory(logDirectory);

            var logMessage =
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{log.Level}] " +
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
                File.AppendAllText(logFile, logMessage + Environment.NewLine);// creates the file
            }

            return Task.CompletedTask;
        }

        public Task LogMessageAsync(string message, string level = "Information")
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Items["LogMessage"] = message;
                context.Items["LogLevel"] = level;
            }
            return Task.CompletedTask;
        }
    }
}