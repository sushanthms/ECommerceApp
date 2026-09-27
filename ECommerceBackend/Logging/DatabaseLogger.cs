using ECommerceBackend.Data;

namespace ECommerceBackend.Logging
{
    public class DatabaseLogger : IApplicationLogger
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DatabaseLogger(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            _scopeFactory = scopeFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(ApplicationLog log)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.ApplicationLogs.Add(log);
            await db.SaveChangesAsync();
        }

        public Task LogMessageAsync(string message, string level = "Information")
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Task.CompletedTask;

            if (!httpContext.Items.ContainsKey("LogMessages"))
            {
                httpContext.Items["LogMessages"] = new List<(string Message, string Level)>();
            }

            var messages = (List<(string Message, string Level)>)httpContext.Items["LogMessages"]!;
            messages.Add((message, level));

            return Task.CompletedTask;
        }
    }
}