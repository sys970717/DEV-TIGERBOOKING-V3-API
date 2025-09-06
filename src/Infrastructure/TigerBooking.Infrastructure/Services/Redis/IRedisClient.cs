using System.Collections.Generic;

namespace TigerBooking.Infrastructure.Services.Redis;

public interface IRedisClient
{
    Task<string?> StringGetAsync(string key);
    Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null);
    Task<bool> KeyDeleteAsync(string key);
    Task<bool> KeyExistsAsync(string key);
    Task<IEnumerable<string>> GetKeysAsync(string pattern);
}
