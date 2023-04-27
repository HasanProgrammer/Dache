using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dache;

public static class IConfigurationExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="hostEnvironment"></param>
    /// <returns></returns>
    public static string GetRedisConnectionString(this IConfiguration configuration,
        IHostEnvironment hostEnvironment
    ) => configuration.GetValue<string>(
        $"Database:Redis.DB:{hostEnvironment.EnvironmentName}.ConnectionString"
    );
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="hostEnvironment"></param>
    /// <returns></returns>
    public static string GetRedisConnectionString(this IConfiguration configuration,
        string hostEnvironment
    ) => configuration.GetValue<string>(
        $"Database:Redis.DB:{hostEnvironment}.ConnectionString"
    );
}