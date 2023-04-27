using System.Reflection;
using Dache.Common;
using Dache.Contracts;
using Karami.Core.UseCase.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Dache.Concretes;

public class CacheService : ICacheService
{
    private readonly IServiceProvider _serviceProvider;

    public CacheService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;
    
    public TResult Get<TResult>()
    {
        object cacheHandler = _serviceProvider.GetRequiredService<IMemoryCacheSetter<TResult>>();

        var cacheHandlerType   = cacheHandler.GetType();
        var cacheHandlerMethod = cacheHandlerType.GetMethod("Set") ?? throw new Exception("Set function not found !");

        var cacheHandlerMethodAttr = cacheHandlerMethod.GetCustomAttribute(typeof(ConfigAttribute)) as ConfigAttribute
                                     ?? 
                                     throw new Exception("CachingAttribute's attribute for set function not found !");
        
        
        var redisCache = _serviceProvider.GetRequiredService<IRedisCache>();
        var cachedData = redisCache.GetCacheValue(cacheHandlerMethodAttr.Key);
        
        if (string.IsNullOrEmpty(cachedData))
        {
            var result = (TResult)cacheHandlerMethod.Invoke(cacheHandler, null);

            redisCache.SetCacheValue(
                new KeyValuePair<string, string>(cacheHandlerMethodAttr.Key, JsonConvert.SerializeObject(result) ) ,
                TimeSpan.FromMinutes( cacheHandlerMethodAttr.TimeOut )
            );
        }

        return JsonConvert.DeserializeObject<TResult>(redisCache.GetCacheValue(cacheHandlerMethodAttr.Key));
    }

    public async Task<TResult> GetAsync<TResult>(CancellationToken cancellationToken)
    {
        object cacheHandler = _serviceProvider.GetRequiredService<IMemoryCacheSetter<TResult>>();

        var cacheHandlerType   = cacheHandler.GetType();
        var cacheHandlerMethod = cacheHandlerType.GetMethod("SetAsync") ?? throw new Exception("SetAsync function not found !");

        var cacheHandlerMethodAttr = cacheHandlerMethod.GetCustomAttribute(typeof(ConfigAttribute)) as ConfigAttribute
                                     ?? 
                                     throw new Exception("CachingAttribute's attribute for set function not found !");
        
        
        var redisCache = _serviceProvider.GetRequiredService<IRedisCache>();
        var cachedData = await redisCache.GetCacheValueAsync(cacheHandlerMethodAttr.Key, cancellationToken);
        
        if (string.IsNullOrEmpty(cachedData))
        {
            var result = await ( cacheHandlerMethod.Invoke(cacheHandler, new object[] { cancellationToken }) as Task<TResult> );

            await redisCache.SetCacheValueAsync(
                new KeyValuePair<string, string>(cacheHandlerMethodAttr.Key, JsonConvert.SerializeObject(result) ) ,
                TimeSpan.FromMinutes( cacheHandlerMethodAttr.TimeOut ) ,
                cancellationToken
            );
        }

        return JsonConvert.DeserializeObject<TResult>(
            await redisCache.GetCacheValueAsync(cacheHandlerMethodAttr.Key, cancellationToken)
        );
    }
}