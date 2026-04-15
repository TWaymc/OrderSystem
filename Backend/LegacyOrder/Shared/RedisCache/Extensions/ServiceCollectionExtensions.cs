
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedisCache.Service;
using StackExchange.Redis;

namespace RedisCache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis") 
                               ?? throw new InvalidOperationException("Redis connection string not configured.");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(connectionString));

        services.AddSingleton<IRedisCacheService, RedisCacheService>();

        return services;
    }
}