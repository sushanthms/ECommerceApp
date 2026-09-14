using ECommerceBackend.Data;

namespace ECommerceBackend.Logging
{
    public class DatabaseLogger : IApplicationLogger
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DatabaseLogger(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(ApplicationLog log)
        {
            _context.ApplicationLogs.Add(log);

            await _context.SaveChangesAsync();
        }

        public Task LogMessageAsync(string message, string level = "Information")
        {
            _httpContextAccessor.HttpContext!.Items["LogMessage"] = message;
            _httpContextAccessor.HttpContext!.Items["LogLevel"] = level;

            return Task.CompletedTask;
        }
    }
}