using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Interfaces;
using Orders.Infrastructure.Data;

namespace Orders.Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly AppDbContext _context;

    public OrderItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderItem?> GetByOrderAndProductAsync(Guid orderId, Guid productId)
    {
        return await _context.OrderItems
            .FirstOrDefaultAsync(i => i.OrderId == orderId && i.ProductId == productId);
    }

    public async Task<OrderItem> AddAsync(OrderItem orderItem)
    {
        orderItem.ModifiedAt = null;
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
        return orderItem;
    }

    public async Task<OrderItem> UpdateAsync(OrderItem orderItem)
    {
        orderItem.ModifiedAt = DateTime.UtcNow;
        _context.OrderItems.Update(orderItem);
        await _context.SaveChangesAsync();
        return orderItem;
    }
}
