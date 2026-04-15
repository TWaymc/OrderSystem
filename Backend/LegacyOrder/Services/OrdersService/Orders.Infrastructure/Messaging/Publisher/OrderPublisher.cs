using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Orders.Infrastructure.Messaging.Events;
using Orders.Infrastructure.Messaging.Publisher.Interface;
using RabbitMQ.Client;

namespace Orders.Infrastructure.Messaging.Publisher;

public class OrderPublisher : IOrderPublisher, IDisposable
{
    private const string DefaultExchangeName = "order-updated-exchange";
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;

    public OrderPublisher(IConfiguration configuration)
    {
        var hostName = configuration["RabbitMq:HostName"] ?? "localhost";
        var port = int.TryParse(configuration["RabbitMq:Port"], out var parsedPort) ? parsedPort : 5672;
        var userName = configuration["RabbitMq:UserName"];
        var password = configuration["RabbitMq:Password"];

        _exchangeName = configuration["RabbitMq:OrderUpdatedExchangeName"]
                        ?? configuration["RabbitMq:ExchangeName"]
                        ?? DefaultExchangeName;
        var exchangeType = configuration["RabbitMq:ExchangeType"] ?? ExchangeType.Fanout;
        var exchangeDurable = bool.TryParse(configuration["RabbitMq:ExchangeDurable"], out var exchangeDurableValue) && exchangeDurableValue;

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port
        };

        if (!string.IsNullOrWhiteSpace(userName))
            factory.UserName = userName;
        if (!string.IsNullOrWhiteSpace(password))
            factory.Password = password;

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(_exchangeName, exchangeType, durable: exchangeDurable, autoDelete: false);
    }

    public async Task PublishAsync(Guid orderId)
    {
        await PublishAsync(new OrderUpdatedEvent
        {
            OrderId = orderId
        });
    }

    private Task PublishAsync(OrderUpdatedEvent orderEvent)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderEvent));

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: string.Empty,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
