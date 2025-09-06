using StackExchange.Redis;
using System.Text.Json;

namespace TigerBooking.Infrastructure.Services.Redis;

public class RedisClientWrapper : IRedisClient
{
    private readonly IConnectionMultiplexer _mux;
    private readonly IDatabase _db;

    public RedisClientWrapper(IConnectionMultiplexer mux)
    {
        _mux = mux;
        _db = _mux.GetDatabase();
    }

    public async Task<string?> StringGetAsync(string key)
    {
        var v = await _db.StringGetAsync(key);
        return v.IsNullOrEmpty ? null : v.ToString();
    }

    public async Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null)
    {
        return await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<bool> KeyDeleteAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    public async Task<IEnumerable<string>> GetKeysAsync(string pattern)
    {
        var server = _mux.GetServer(_mux.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern).Select(k => (string)k).ToList();
        return keys;
    }
}
