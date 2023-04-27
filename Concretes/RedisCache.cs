using Dache.Contracts;
using StackExchange.Redis;

namespace Dache.Concretes;

public class RedisCache : IRedisCache
{
    private readonly IDatabase _redisDB;
        
    public RedisCache(IConnectionMultiplexer connection) => _redisDB = connection.GetDatabase();

    public string GetCacheValue(string key) => _redisDB.StringGet(key);

    public async Task<string> GetCacheValueAsync(string key, CancellationToken cancellationToken) 
        => await _redisDB.StringGetAsync(key);

    public void SetCacheValue(KeyValuePair<string, string> keyValue, TimeSpan time)
        => _redisDB.StringSet(keyValue.Key, keyValue.Value, time);

    public async Task SetCacheValueAsync(KeyValuePair<string, string> keyValue, TimeSpan time, 
        CancellationToken cancellationToken
    ) => await _redisDB.StringSetAsync(keyValue.Key, keyValue.Value, time);

    public bool DeleteKey(string key) => GetCacheValue(key) is not null && _redisDB.KeyDelete(key);

    public async Task<bool> DeleteKeyAsync(string key, CancellationToken cancellationToken) 
        => await GetCacheValueAsync(key, cancellationToken) is not null && _redisDB.KeyDelete(key);
}