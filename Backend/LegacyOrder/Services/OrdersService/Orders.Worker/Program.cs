using Orders.Worker;
using Orders.Worker.Messaging;
using RabbitMQ.Client;
using RedisCache.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRedisService(builder.Configuration);

builder.Services.AddSingleton<IConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var hostName = configuration["RabbitMq:HostName"]
                   ?? throw new InvalidOperationException("RabbitMq:HostName is not configured.");
    var userName = configuration["RabbitMq:UserName"]
                   ?? throw new InvalidOperationException("RabbitMq:UserName is not configured.");
    var password = configuration["RabbitMq:Password"]
                   ?? throw new InvalidOperationException("RabbitMq:Password is not configured.");
    var port = int.TryParse(configuration["RabbitMq:Port"], out var parsedPort) ? parsedPort : 5672;

    var factory = new ConnectionFactory
    {
        HostName = hostName,
        Port = port,
        UserName = userName,
        Password = password
    };

    return factory.CreateConnection();
});

builder.Services.AddSingleton<CacheInvalidationConsumer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();