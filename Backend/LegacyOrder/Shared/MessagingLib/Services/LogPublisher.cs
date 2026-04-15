using System.Text;
using System.Text.Json;
using LoggingLib.Models;
using LoggingLib.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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

    public LogPublisher(IHttpContextAccessor httpContextAccessor, string defaultServiceName = "")
    {
        _httpContextAccessor = httpContextAccessor;
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        _defaultServiceName = defaultServiceName;

        _connection = factory.CreateConnection();
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

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}