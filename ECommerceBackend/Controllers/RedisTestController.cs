using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedisTestController : ControllerBase
{
    private readonly IDistributedCache _cache;

    public RedisTestController(IDistributedCache cache)// gives the controller access to the caching service registered in Program.cs.
    {
        _cache = cache;
    }

    [HttpGet("set")]
    public async Task<IActionResult> Set()
    {
        await _cache.SetStringAsync("redis-test", "Hello from ASP.NET Core!",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            }
            );

        return Ok("Value saved in Redis with 2-minute expiration.");
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get()
    {
        var value = await _cache.GetStringAsync("redis-test");

        return Ok(value ?? "No value found in Redis.");
    }


    [HttpGet("set-sliding")]
    public async Task<IActionResult> SetSliding()
    {
        await _cache.SetStringAsync(
            "redis-test-sliding",
            "Hello from sliding expiration!",
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(2)
            }
        );

        return Ok("Value saved with 2-minute sliding expiration.");
    }

    [HttpGet("get-sliding")]
    public async Task<IActionResult> GetSliding()
    {
        var value = await _cache.GetStringAsync("redis-test-sliding");

        return Ok(value ?? "No value found in Redis.");
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove()
    {
        await _cache.RemoveAsync("redis-test");

        return Ok("Cached value removed.");
    }

}
