using Products.Application.DTOs;

namespace Products.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();

        Task<ProductDto?> GetByIdAsync(Guid id);

        Task<ProductDto> CreateAsync(CreateProductDto productDto, string createdBy);

        Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, string lastModifiedBy);

        Task<bool> DeleteAsync(Guid id, string lastModifiedBy);
    }
}