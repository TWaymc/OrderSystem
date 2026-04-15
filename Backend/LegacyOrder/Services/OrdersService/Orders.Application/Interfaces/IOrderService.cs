using Orders.Application.DTOs;

namespace Orders.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, string createdBy);
    Task<OrderDto> UpdateAsync(Guid id, UpdateOrderDto dto, string lastModifiedBy);
    Task<OrderDto> AddOrderItemAsync(Guid orderId, AddOrderItemDto dto, string lastModifiedBy);
    Task<bool> RemoveOrderItemAsync(Guid orderId, Guid orderItemId, string lastModifiedBy);
    Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, string lastModifiedBy);
    Task<bool> DeleteAsync(Guid id, string lastModifiedBy);
}
