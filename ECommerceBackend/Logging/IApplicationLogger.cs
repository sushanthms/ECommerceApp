namespace ECommerceBackend.Logging
{
    public interface IApplicationLogger // I means interface, helps to read
    {
        Task LogAsync(ApplicationLog log);
    }
}