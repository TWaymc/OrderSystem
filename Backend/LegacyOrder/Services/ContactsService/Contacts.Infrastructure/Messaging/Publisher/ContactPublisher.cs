using System.Text;
using System.Text.Json;
using Contacts.Infrastructure.Messaging.Events;
using Contacts.Infrastructure.Messaging.Publisher.Interface;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Contacts.Infrastructure.Messaging.Publisher;

public class ContactPublisher : IContactPublisher, IDisposable
{
    private const string DefaultExchangeName = "contact-updated-exchange";
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;

    public ContactPublisher(IConfiguration configuration)
    {
        var hostName = configuration["RabbitMq:HostName"] ?? "localhost";
        var port = int.TryParse(configuration["RabbitMq:Port"], out var parsedPort) ? parsedPort : 5672;
        var userName = configuration["RabbitMq:UserName"];
        var password = configuration["RabbitMq:Password"];

        _exchangeName = configuration["RabbitMq:ContactUpdatedExchangeName"]
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

    public async Task PublishAsync(Guid contactId)
    {
        await PublishAsync(new ContactUpdatedEvent
        {
            ContactId = contactId
        });
    }

    private Task PublishAsync(ContactUpdatedEvent contactEvent)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(contactEvent));

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
