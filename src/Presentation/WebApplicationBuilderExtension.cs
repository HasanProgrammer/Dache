using System.Reflection;
using Dache.Concretes;
using Dache.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Dache;

public static class WebApplicationBuilderExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="assemblyName"></param>
    public static void RegisterCaching(this WebApplicationBuilder builder, string assemblyName)
    {
        Type[] useCaseAssemblyTypes = Assembly.Load(new AssemblyName(assemblyName)).GetTypes();
        
        //Third party ( Redis )
        builder.Services.AddScoped<IConnectionMultiplexer>(
            Provider => ConnectionMultiplexer.Connect(
                builder.Configuration.GetRedisConnectionString(builder.Environment)
            )
        );
        
        //Pure
        builder.Services.AddScoped(
            typeof(IRedisCache),
            typeof(RedisCache)
        );
        
        //Pure ( Mediator for cache )
        builder.Services.AddTransient(
            typeof(ICacheService),
            typeof(CacheService)
        );
        
        RegisterAllCachesHandler(builder.Services, useCaseAssemblyTypes);
    }

    #region AutoRegisterCacheHandler

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="useCaseAssemblyTypes"></param>
    private static void RegisterAllCachesHandler(IServiceCollection serviceCollection, Type[] useCaseAssemblyTypes)
    {
        var cacheHandlerTypes = useCaseAssemblyTypes.Where(
            type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMemoryCacheSetter<>))
        );

        foreach (Type cacheHandlerType in cacheHandlerTypes) {
                
            Type cacheHandlerTypeValue = cacheHandlerType.GetInterfaces().FirstOrDefault().GetGenericArguments().FirstOrDefault();
                
            serviceCollection.AddTransient(
                typeof(IMemoryCacheSetter<>).MakeGenericType(cacheHandlerTypeValue),
                cacheHandlerType
            );
            
        }
    }

    #endregion
}