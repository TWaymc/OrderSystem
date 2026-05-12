using System.Text;
using System.Text.Json;
using LoggingLib.Models;
using LoggingLib.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace LoggingLib.Services;

public class LogPublisher : ILogPublisher, IDisposable
{
    private const string ExchangeName = "logs-exchange";
    
    private const string CorrelationIdItemKey = "CorrelationId";

    private readonly string _defaultServiceName = "";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public LogPublisher(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        string defaultServiceName = "")
    {
        _httpContextAccessor = httpContextAccessor;

        var hostName = configuration["RabbitMq:HostName"] ?? "localhost";
        var port = int.TryParse(configuration["RabbitMq:Port"], out var parsedPort) ? parsedPort : 5672;
        var userName = configuration["RabbitMq:UserName"];
        var password = configuration["RabbitMq:Password"];

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = string.IsNullOrWhiteSpace(userName) ? ConnectionFactory.DefaultUser : userName,
            Password = string.IsNullOrWhiteSpace(password) ? ConnectionFactory.DefaultPass : password,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        _defaultServiceName = defaultServiceName;

        _connection = CreateConnectionWithRetry(factory);
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: false, autoDelete: false);
    }

    public Task PublishAsync(LogMessage log)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(log));

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: string.Empty,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    public Task ErrorAsync(string message, string? exception = null) =>
        PublishAsync(new LogMessage
        {
            ServiceName = _defaultServiceName,
            Level = "Error",
            Message = message,
            Exception = exception,
            Timestamp = DateTime.UtcNow,
            CorrelationId = TryGetCorrelationId()
        });

    public Task WarningAsync(string message, string? exception = null) =>
        PublishAsync(new LogMessage
        {
            ServiceName = _defaultServiceName,
            Level = "Warning",
            Message = message,
            Exception = exception,
            Timestamp = DateTime.UtcNow,
            CorrelationId = TryGetCorrelationId()
        });
    
    public Task InfoAsync(string message, string? exception = null) =>
        PublishAsync(new LogMessage
        {
            ServiceName = _defaultServiceName,
            Level = "Information",
            Message = message,
            Exception = exception,
            Timestamp = DateTime.UtcNow,
            CorrelationId = TryGetCorrelationId()
        });
    
    //

    private string? TryGetCorrelationId()
    {
        if (_httpContextAccessor.HttpContext?.Items.TryGetValue(CorrelationIdItemKey, out var value) == true &&
            value is string id &&
            !string.IsNullOrWhiteSpace(id))
            return id;
        return null;
    }

    private static IConnection CreateConnectionWithRetry(ConnectionFactory factory)
    {
        const int maxAttempts = 20;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return factory.CreateConnection();
            }
            catch (Exception ex)
            {
                lastException = ex;
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }

        throw new InvalidOperationException(
            "Unable to connect to RabbitMQ after multiple retry attempts.",
            lastException);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}