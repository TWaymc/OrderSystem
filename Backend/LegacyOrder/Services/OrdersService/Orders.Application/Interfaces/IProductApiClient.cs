using Orders.Application.DTOs;

namespace Orders.Application.Interfaces;

public interface IProductApiClient
{
    Task<ProductDto?> GetByIdAsync(Guid id);
}