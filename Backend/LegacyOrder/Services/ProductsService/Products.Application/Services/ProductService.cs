using AutoMapper;
using LoggingLib.Services.Interfaces;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Infrastructure.Messaging.Publisher.Interface;
using RedisCache.Service;

namespace Products.Application.Services;

public class ProductService: IProductService
{
    private readonly IProductRepository _repo;
    private readonly ISequenceService _sequences;
    private readonly IMapper _mapper;
    private readonly ILogPublisher _logger;
    private readonly IProductPublisher _productPublisher;
    private readonly IRedisCacheService _cache;

    public ProductService(
        IProductRepository repo, 
        ISequenceService sequences, 
        IMapper mapper, 
        ILogPublisher logger,
        IProductPublisher productPublisher,
        IRedisCacheService cache)
    {
        _repo = repo;
        _sequences = sequences;
        _mapper = mapper;
        _logger = logger;
        _productPublisher = productPublisher;
        _cache = cache;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var productDto = await _cache.GetAsync<ProductDto>($"products:product:{id}");

        if (productDto != null)
        {
            return productDto;
        } 
        var product = await _repo.GetByIdAsync(id);
        if (product == null) return null;
            
        var res = _mapper.Map<ProductDto>(product);
            
        await _cache.SetAsync($"products:product:{id}", res, TimeSpan.FromMinutes(30));

        return res;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, string createdBy)
    {
        var code = await _sequences.GetNextProductCodeAsync();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = dto.Name,
            Price = dto.Price,
            Description = dto.Description,
            CreatedBy = createdBy,
            LastModifiedBy = createdBy
        };

        var created = await _repo.AddAsync(product);
        
        await _logger.InfoAsync($"New Product: {code} created by: {createdBy}");
        
        return _mapper.Map<ProductDto>(created);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, string lastModifiedBy)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product == null)
            throw new Exception("Product not found");

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Description = dto.Description;
        product.LastModifiedBy = lastModifiedBy;

        var updated = await _repo.UpdateAsync(product);
        
        await _productPublisher.PublishAsync(id);
        
        await _cache.RemoveAsync($"products:product:{id}");
        
        await _logger.InfoAsync($"Product: {product.Code} modified by: {lastModifiedBy}");
        
        return _mapper.Map<ProductDto>(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, string lastModifiedBy)
    {
        var res = await _repo.DeleteAsync(id, lastModifiedBy);
        
        if (res)
        {
            await _cache.RemoveAsync($"products:product:{id}");
            
            await _productPublisher.PublishAsync(id);
        }

        return res;
    }
}