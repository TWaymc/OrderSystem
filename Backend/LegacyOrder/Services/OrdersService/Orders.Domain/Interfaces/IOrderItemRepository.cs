using Orders.Domain.Entities;

namespace Orders.Domain.Interfaces;

public interface IOrderItemRepository
{
    Task<OrderItem?> GetByOrderAndProductAsync(Guid orderId, Guid productId);
    Task<OrderItem> AddAsync(OrderItem orderItem);
    Task<OrderItem> UpdateAsync(OrderItem orderItem);
}
