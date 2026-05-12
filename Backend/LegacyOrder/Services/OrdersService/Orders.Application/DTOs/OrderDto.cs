using Orders.Domain.Entities;

namespace Orders.Application.DTOs;

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal ProductUnitPrice,
    int Quantity)
{
    public decimal SubTotal => ProductUnitPrice * Quantity;
}

public record OrderDto(
    Guid Id,
    string Code,
    OrderStatus StatusCode,
    Guid CustomerId,
    string CustomerName,
    string CustomerSurname,
    string? CustomerMobileNumber,
    string? CustomerEmail,
    IReadOnlyList<OrderItemDto> OrderItems,
    decimal TotalAmount,
    int TotalItems,
    string CreatedBy,
    string LastModifiedBy,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    string RowVersion);

public record CreateOrderItemDto(
    Guid ProductId,
    int Quantity);

public record CreateOrderDto(
    Guid CustomerId,
    IReadOnlyList<CreateOrderItemDto> OrderItems);

public record UpdateOrderDto(
    Guid CustomerId,
    string RowVersion);

public record AddOrderItemDto(
    Guid ProductId,
    int Quantity);

public record UpdateOrderStatusDto(
    OrderStatus StatusCode,
    string RowVersion);
