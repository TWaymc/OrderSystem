using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RedisCache.Service;

namespace Orders.Worker.Messaging;

public class CacheInvalidationConsumer : IDisposable
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<CacheInvalidationConsumer> _logger;
    private readonly IConfiguration _configuration;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CacheInvalidationConsumer(
        IConnection connection,
        IRedisCacheService cache,
        IConfiguration configuration,
        ILogger<CacheInvalidationConsumer> logger)
    {
        _connection = connection;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;

        var productExchange = _configuration["RabbitMq:ProductUpdatedExchangeName"] ?? "product-updated-exchange";
        var contactExchange = _configuration["RabbitMq:ContactUpdatedExchangeName"] ?? "contact-updated-exchange";
        var exchangeType = _configuration["RabbitMq:ExchangeType"] ?? ExchangeType.Fanout;

        var exchangeDurable = bool.TryParse(_configuration["RabbitMq:ExchangeDurable"], out var exchangeDurableValue) && exchangeDurableValue;
        var queueDurable = bool.TryParse(_configuration["RabbitMq:QueueDurable"], out var queueDurableValue) && queueDurableValue;
        var prefetchCount = ushort.TryParse(_configuration["RabbitMq:PrefetchCount"], out var prefetchCountValue) ? prefetchCountValue : (ushort)10;

        var productQueue = _configuration["RabbitMq:ProductUpdatedQueueName"] ?? "orders-product-cache-invalidation-queue";
        var contactQueue = _configuration["RabbitMq:ContactUpdatedQueueName"] ?? "orders-contact-cache-invalidation-queue";

        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(productExchange, exchangeType, durable: exchangeDurable, autoDelete: false);
        _channel.ExchangeDeclare(contactExchange, exchangeType, durable: exchangeDurable, autoDelete: false);

        _channel.QueueDeclare(productQueue, durable: queueDurable, exclusive: false, autoDelete: false);
        _channel.QueueDeclare(contactQueue, durable: queueDurable, exclusive: false, autoDelete: false);

        _channel.QueueBind(productQueue, productExchange, string.Empty);
        _channel.QueueBind(contactQueue, contactExchange, string.Empty);

        _channel.BasicQos(0, prefetchCount, false);
    }

    public void Start()
    {
        StartProductConsumer();
        StartContactConsumer();
    }

    private void StartProductConsumer()
    {
        var queueName = _configuration["RabbitMq:ProductUpdatedQueueName"] ?? "orders-product-cache-invalidation-queue";
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<ProductUpdatedEvent>(json, JsonOptions);
                if (evt == null || evt.ProductId == Guid.Empty)
                    throw new InvalidOperationException("Invalid product update event payload.");

                await _cache.RemoveAsync($"orders:product:{evt.ProductId}");
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process product cache invalidation event.");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    private void StartContactConsumer()
    {
        var queueName = _configuration["RabbitMq:ContactUpdatedQueueName"] ?? "orders-contact-cache-invalidation-queue";
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<ContactUpdatedEvent>(json, JsonOptions);
                if (evt == null || evt.ContactId == Guid.Empty)
                    throw new InvalidOperationException("Invalid contact update event payload.");

                await _cache.RemoveAsync($"orders:contact:{evt.ContactId}");
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process contact cache invalidation event.");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    private sealed class ProductUpdatedEvent
    {
        public Guid ProductId { get; set; }
    }

    private sealed class ContactUpdatedEvent
    {
        public Guid ContactId { get; set; }
    }
}
