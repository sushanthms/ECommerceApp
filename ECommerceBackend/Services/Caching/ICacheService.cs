
namespace ECommerceBackend.Services.Caching;

public interface ICacheService
{
    Task<string?> GetStringAsync(string key);

    Task SetStringAsync(string key, string value, TimeSpan expiration);

    Task RemoveAsync(string key);
}
