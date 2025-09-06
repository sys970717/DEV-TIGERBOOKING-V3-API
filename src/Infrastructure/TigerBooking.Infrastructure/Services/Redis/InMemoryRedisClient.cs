using System.Collections.Concurrent;

namespace TigerBooking.Infrastructure.Services.Redis;

public class InMemoryRedisClient : IRedisClient
{
    private readonly ConcurrentDictionary<string, string> _store = new();

    public Task<string?> StringGetAsync(string key)
    {
        if (_store.TryGetValue(key, out var v)) return Task.FromResult<string?>(v);
        return Task.FromResult<string?>(null);
    }

    public Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null)
    {
        _store[key] = value;
        return Task.FromResult(true);
    }

    public Task<bool> KeyDeleteAsync(string key)
    {
        return Task.FromResult(_store.TryRemove(key, out _));
    }

    public Task<bool> KeyExistsAsync(string key)
    {
        return Task.FromResult(_store.ContainsKey(key));
    }

    public Task<IEnumerable<string>> GetKeysAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return Task.FromResult<IEnumerable<string>>(_store.Keys.ToList());
        if (pattern.EndsWith("*"))
        {
            var prefix = pattern.TrimEnd('*');
            var keys = _store.Keys.Where(k => k.StartsWith(prefix)).ToList();
            return Task.FromResult<IEnumerable<string>>(keys);
        }
        return Task.FromResult<IEnumerable<string>>(_store.Keys.Where(k => k == pattern).ToList());
    }
}
