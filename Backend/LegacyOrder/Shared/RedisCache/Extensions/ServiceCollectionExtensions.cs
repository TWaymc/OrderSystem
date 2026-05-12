
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedisCache.Service;
using StackExchange.Redis;
using System.Linq;

namespace RedisCache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis") 
                               ?? throw new InvalidOperationException("Redis connection string not configured.");

        var redisOptions = ConfigurationOptions.Parse(connectionString);
        redisOptions.AbortOnConnectFail = false;
        redisOptions.ConnectRetry = 5;
        redisOptions.ConnectTimeout = 10000;
        redisOptions.SyncTimeout = 10000;

        if (!redisOptions.EndPoints.Any())
        {
            redisOptions.EndPoints.Add("redis", 6379);
        }

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisOptions));

        services.AddSingleton<IRedisCacheService, RedisCacheService>();

        return services;
    }
}