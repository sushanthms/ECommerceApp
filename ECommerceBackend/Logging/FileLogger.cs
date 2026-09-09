namespace ECommerceBackend.Logging
{
    public class FileLogger : IApplicationLogger
    {
        private readonly object _lock = new object();

        public Task LogAsync(ApplicationLog log)
        {
            var logDirectory = "Logs";
            var logFile = Path.Combine(logDirectory, "ecommerce.log");


            Directory.CreateDirectory(logDirectory);

            var logMessage =
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [Information] " +
                $"HTTP Request | " +
                $"Method: {log.HttpMethod} | " +
                $"Path: {log.RequestPath} | " +
                $"Query: {log.QueryString} | " +
                $"StartTime: {log.RequestStartTime} | " +
                $"StatusCode: {log.ResponseStatusCode} | " +
                $"Duration: {log.ExecutionDuration}ms | " +
                $"ClientIP: {log.ClientIp} | " +
                $"CorrelationId: {log.CorrelationId}";

            lock (_lock)
            {
                File.AppendAllText(
                    logFile,
                    logMessage + Environment.NewLine
                );
            }

            return Task.CompletedTask;
        }
    }
}