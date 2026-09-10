namespace ECommerceBackend.Logging
{
    public interface IApplicationLogger
    {
        Task LogAsync(ApplicationLog log);
        Task LogMessageAsync(string message);
    }
}