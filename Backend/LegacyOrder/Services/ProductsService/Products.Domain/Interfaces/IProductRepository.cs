using Products.Domain.Entities;

namespace Products.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product, byte[] originalRowVersion);
    Task<bool> DeleteAsync(Guid id, string lastModifiedBy);
}