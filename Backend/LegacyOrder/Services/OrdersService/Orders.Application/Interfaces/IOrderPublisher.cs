namespace Orders.Infrastructure.Messaging.Publisher.Interface;

public interface IOrderPublisher
{
    Task PublishAsync(Guid orderId);
}
