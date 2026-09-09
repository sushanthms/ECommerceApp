using ECommerceBackend.Data;

namespace ECommerceBackend.Logging
{
    public class DatabaseLogger : IApplicationLogger
    {
        private readonly AppDbContext _context;

        public DatabaseLogger(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(ApplicationLog log)
        {
            _context.ApplicationLogs.Add(log);

            await _context.SaveChangesAsync();
        }
    }
}