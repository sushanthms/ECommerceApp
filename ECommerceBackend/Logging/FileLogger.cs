namespace ECommerceBackend.Logging
{
    public class FileLogger : IApplicationLogger
    {
        private static readonly object _lock = new object();// empty C# object whose purpose here is to act as a lock/synchronization marker.
        // without static each filelogger object will get its own separate lock, so they wouldnot coordinate correcly
        // so static means there is one _lock associated with the FileLogger class
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
                $"SessionId: {log.SessionId} | " +
                $"ExceptionType: {log.ExceptionType} | " +
                $"ExceptionMessage: {log.ExceptionMessage} | " +
                $"StackTrace: {log.StackTrace}";

            lock (_lock)// only one thread at a time can run the code inside this
            {
                File.AppendAllText(logFile, logMessage + Environment.NewLine);// creates the file
            }// AppendAllText means adds the text to the end of the file
            // Environment is a .NET class that provides information related to the environment in which the application is running.
            // NewLine is a property of the Environment class. Environment.NewLine asks to give the correct new-line character(s) for the current operating system.

            return Task.CompletedTask;
        }

        public Task LogMessageAsync(string message, string level = "Information")// puts the message and level from the controller to the HttpContext
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