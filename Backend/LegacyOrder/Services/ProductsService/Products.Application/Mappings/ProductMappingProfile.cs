using AutoMapper;
using Products.Application.DTOs;
using Products.Domain.Entities;

namespace Products.Application.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForCtorParam("RowVersion", opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));
    }
}
