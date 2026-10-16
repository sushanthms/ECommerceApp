using ECommerceBackend.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;

namespace ECommerceBackend.Services.Caching;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IApplicationLogger _logger;

    private static DateTime _redisPausedUntil = DateTime.MinValue;

    private static readonly TimeSpan RedisPauseDuration = TimeSpan.FromSeconds(30);

    private static bool IsRedisPaused => DateTime.UtcNow < _redisPausedUntil;
    private static readonly ConcurrentDictionary<string, byte>PendingInvalidations = new();

    public CacheService(IDistributedCache cache, IApplicationLogger logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> GetStringAsync(string key)
    {
        if (IsRedisPaused)
            return null;

        try
        {
            var invalidationsProcessed = await ProcessPendingInvalidationsAsync();

            if (!invalidationsProcessed)
                return null;

            return await _cache.GetStringAsync(key);
        }
        catch (Exception ex)
        {
            PauseRedis();
            await _logger.LogMessageAsync($"Redis read failed for key {key}: {ex.Message}");
            return null;
        }
    }

    public async Task SetStringAsync(string key, string value, TimeSpan expiration)
    {
        if (IsRedisPaused)// is redispaused will be true here, because _redisPausedUntil has value 30 seconds and IsRedisPaused become true when it is checked here.
            return;// SetStringAsync after GetStringAsync failed in ProdcutService

        try
        {
            await _cache.SetStringAsync(key, value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration// expiration value is from the parameter
                });
        }
        catch (Exception ex)
        {
            PauseRedis();
            await _logger.LogMessageAsync($"Redis write failed for key {key}: {ex.Message}");
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (IsRedisPaused)
        {
            PendingInvalidations.TryAdd(key, 0);// 0 is a value for the value field, it is not real value, we write it because the method like TryAdd, TryRemove wants two fields
            return;
        }

        try
        {
            await _cache.RemoveAsync(key);
            PendingInvalidations.TryRemove(key, out _);// we get key and value fields, but we dont want the value field, so we get the value field in the _ then we discard it
        }
        catch (Exception ex)
        {
            PendingInvalidations.TryAdd(key, 0);
            PauseRedis();
            await _logger.LogMessageAsync($"Redis removal failed for key {key}: {ex.Message}");
        }
    }

    private static void PauseRedis()
    {
        _redisPausedUntil = DateTime.UtcNow.Add(RedisPauseDuration);
    }


    private async Task<bool> ProcessPendingInvalidationsAsync()
    {
        foreach (var key in PendingInvalidations.Keys)
        {
            try
            {
                await _cache.RemoveAsync(key);
                PendingInvalidations.TryRemove(key, out _);
            }
            catch (Exception ex)
            {
                PauseRedis();

                await _logger.LogMessageAsync(
                    $"Pending Redis invalidation failed for key {key}: {ex.Message}");

                return false;
            }
        }

        return true;
    }

}
