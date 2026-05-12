using AutoMapper;
using Orders.Application.DTOs;
using Orders.Domain.Entities;

namespace Orders.Application.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForCtorParam("RowVersion", opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));
        CreateMap<OrderItem, OrderItemDto>();
    }
}
