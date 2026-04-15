namespace Orders.Infrastructure.Messaging.Events;

public class OrderUpdatedEvent
{
    public Guid OrderId { get; set; }
}
