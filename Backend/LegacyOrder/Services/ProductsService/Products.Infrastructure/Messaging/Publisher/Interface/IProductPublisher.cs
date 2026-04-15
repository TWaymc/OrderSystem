using Products.Domain.Entities;

namespace Products.Infrastructure.Messaging.Publisher.Interface;

public interface IProductPublisher
{
    Task PublishAsync(Guid productId);
}