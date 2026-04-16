using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Interfaces;
using Orders.Infrastructure.Data;

namespace Orders.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> AddAsync(Order order)
    {
        order.ModifiedAt = null;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        order.ModifiedAt = DateTime.UtcNow;

        //_context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteAsync(Guid id, string lastModifiedBy)
    {
        var order = await _context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null || order.IsDeleted)
            return false;

        order.IsDeleted = true;
        order.ModifiedAt = DateTime.UtcNow;
        order.LastModifiedBy = lastModifiedBy;

        foreach (var item in order.OrderItems)
        {
            item.IsDeleted = true;
            item.ModifiedAt = DateTime.UtcNow;
            item.LastModifiedBy = lastModifiedBy;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
