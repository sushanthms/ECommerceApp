namespace ECommerceBackend.Logging
{
    public class FileLogger : IApplicationLogger
    {
        private static readonly object _lock = new();// empty C# object whose purpose here is to act as a lock/synchronization marker.
        // without static each filelogger object will get its own separate lock, so they wouldnot coordinate correcly
        // so static means there is one _lock associated with the FileLogger class
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _logDirectory;

        public FileLogger(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _httpContextAccessor = httpContextAccessor;
            _logDirectory = Path.Combine(env.ContentRootPath, "Logs");
            Directory.CreateDirectory(_logDirectory);
        }

        public Task LogAsync(ApplicationLog log)
        {
            var now = DateTime.UtcNow;
            var logFile = Path.Combine(_logDirectory, $"ecommerce-{now:yyyy-MM-dd}.log");

            var line =
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{log.Level}] " +
                $"EventName: {log.EventName} | " +
                $"Message: {log.Message} | " +
                $"UserId: {log.UserId} | " +
                $"EntityId: {log.EntityId} | " +
                $"ErrorCode: {log.ErrorCode} | " +
                $"Method: {log.HttpMethod} | " +
                $"Path: {log.RequestPath} | " +
                $"Query: {log.QueryString} | " +
                $"StartTime: {log.RequestStartTime:yyyy-MM-dd HH:mm:ss.fff} | " +
                $"StatusCode: {log.ResponseStatusCode} | " +
                $"Duration: {log.ExecutionDuration}ms | " +
                $"ClientIP: {log.ClientIp} | " +
                $"CorrelationId: {log.CorrelationId} | " +
                $"SessionId: {log.SessionId} | " +
                $"ExceptionType: {log.ExceptionType} | " +
                $"ExceptionMessage: {log.ExceptionMessage} | " +
                $"StackTrace: {log.StackTrace}";

            line = line.Replace("\r", " ").Replace("\n", " ");

            lock (_lock)// only one thread at a time can run the code inside this
            {
                File.AppendAllText(logFile, line + Environment.NewLine);
            }// AppendAllText means adds the text to the end of the file
            // Environment is a .NET class that provides information related to the environment in which the application is running.
            // NewLine is a property of the Environment class. Environment.NewLine asks to give the correct new-line character(s) for the current operating system.

            return Task.CompletedTask;
        }

        public Task LogMessageAsync(string message, string level = "Information")// puts the message and level from the controller to the HttpContext
        {
            var httpContext = _httpContextAccessor.HttpContext;// accessing the current http request

            if (httpContext == null)// this happens if the method is called directly in the terminal(not from the browser)
                return Task.CompletedTask;

            // the list contains each log messages of a http request, that list is stored inside httpcontext.items with the key name LogMessages
            // LogMessages is the name/key used to access the list, not the actual name of the List object.
            if (!httpContext.Items.ContainsKey("LogMessages"))
            {
                httpContext.Items["LogMessages"] = new List<(string Message, string Level)>();
            }
            // List<(string Message, string Level)> is the type and httpContext.Items["LogMessages"]; is the value
            // messages is the reference variable to the list, and when we do messages.add the logmessage is added to the list that is inside httpcontext.items
            var messages = (List<(string Message, string Level)>)httpContext.Items["LogMessages"];
            // messages is the reference to the list
            messages.Add((message, level));

            return Task.CompletedTask;
        }
    }
}