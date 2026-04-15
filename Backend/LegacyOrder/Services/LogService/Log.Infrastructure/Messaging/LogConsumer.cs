using System.Text;
using System.Text.Json;
using Log.Domain.Entities;
using LogService.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Log.Infrastructure.Messaging;
public class LogConsumer : IDisposable
{
    private readonly LogProcessor _processor;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<LogConsumer> _logger;
    private readonly IConfiguration _configuration;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LogConsumer(
        LogProcessor processor,
        IConnection connection,
        IConfiguration configuration,
        ILogger<LogConsumer> logger)
    {
        _processor = processor;
        _connection = connection;
        _configuration = configuration;
        _logger = logger;

        var exchangeName  = _configuration["RabbitMq:ExchangeName"]  ?? throw new InvalidOperationException("RabbitMq:ExchangeName is not configured.");
        var exchangeType  = _configuration["RabbitMq:ExchangeType"]  ?? throw new InvalidOperationException("RabbitMq:ExchangeType is not configured.");
        var queueName     = _configuration["RabbitMq:QueueName"]     ?? throw new InvalidOperationException("RabbitMq:QueueName is not configured.");
        var exchangeDurable = bool.TryParse(_configuration["RabbitMq:ExchangeDurable"], out var exchangeDurableValue) && exchangeDurableValue;
        var queueDurable    = bool.TryParse(_configuration["RabbitMq:QueueDurable"], out var queueDurableValue) && queueDurableValue;
        var prefetchCount   = ushort.TryParse(_configuration["RabbitMq:PrefetchCount"], out var prefetchCountValue) ? prefetchCountValue : (ushort)10;

        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: exchangeName,
            type: exchangeType,
            durable: exchangeDurable,
            autoDelete: false);

        _channel.QueueDeclare(
            queue: queueName,
            durable: queueDurable,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(queueName, exchangeName, "");
        _channel.BasicQos(0, prefetchCount, false);
    }

    public void Start()
    {
        var queueName = _configuration["RabbitMq:QueueName"]!;

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (sender, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var log = JsonSerializer.Deserialize<LogEntry>(json, JsonOptions);

                if (log != null)
                    _processor.Process(log);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process log message");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}