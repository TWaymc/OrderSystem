using AutoMapper;
using Orders.Application.DTOs;
using Orders.Domain.Entities;

namespace Orders.Application.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>();
    }
}
