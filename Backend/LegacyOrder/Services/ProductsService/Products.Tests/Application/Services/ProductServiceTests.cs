using AutoMapper;
using FluentAssertions;
using LoggingLib.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Application.Mappings;
using Products.Application.Services;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using Products.Infrastructure.Messaging.Publisher.Interface;
using RedisCache.Service;

namespace Products.Tests.Application.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<ISequenceService> _sequences = new();
    private readonly Mock<ILogPublisher> _logger = new();
    private readonly Mock<IProductPublisher> _publisher = new();
    private readonly Mock<IRedisCacheService> _cache = new();
    private readonly IMapper _mapper;

    public ProductServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>(), loggerFactory);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistWithSequenceCodeAndMapToDto()
    {
        _sequences.Setup(x => x.GetNextProductCodeAsync(It.IsAny<CancellationToken>())).ReturnsAsync("P-00001");
        _repo.Setup(x => x.AddAsync(It.IsAny<Product>())).ReturnsAsync((Product p) =>
        {
            p.RowVersion = new byte[] { 1, 0, 0, 0 };
            p.CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return p;
        });

        var service = CreateService();
        var dto = new CreateProductDto("Widget", 19.99m, "A widget");

        var result = await service.CreateAsync(dto, "creator");

        result.Code.Should().Be("P-00001");
        result.Name.Should().Be("Widget");
        result.Price.Should().Be(19.99m);
        result.RowVersion.Should().NotBeNullOrEmpty();
        _repo.Verify(x => x.AddAsync(It.Is<Product>(p => p.Code == "P-00001" && p.CreatedBy == "creator")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPassOriginalRowVersionToRepository()
    {
        var id = Guid.NewGuid();
        var token = Convert.ToBase64String(new byte[] { 5, 4, 3, 2 });
        var entity = new Product
        {
            Id = id,
            Code = "P-00002",
            Name = "Old",
            Price = 1m,
            Description = "d",
            RowVersion = new byte[] { 1, 1, 1, 1 }
        };

        _repo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<byte[]>()))
            .ReturnsAsync((Product p, byte[] _) => p);

        var service = CreateService();
        await service.UpdateAsync(id, new UpdateProductDto("New", 2m, "desc", token), "editor");

        _repo.Verify(
            x => x.UpdateAsync(
                It.Is<Product>(p => p.Name == "New" && p.LastModifiedBy == "editor"),
                It.Is<byte[]>(v => v.SequenceEqual(new byte[] { 5, 4, 3, 2 }))),
            Times.Once);
        _publisher.Verify(x => x.PublishAsync(id), Times.Once);
        _cache.Verify(x => x.RemoveAsync($"products:product:{id}"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMiss_ShouldLoadFromRepositoryAndSetCache()
    {
        var id = Guid.NewGuid();
        var entity = new Product
        {
            Id = id,
            Code = "P-00003",
            Name = "Item",
            Price = 10m,
            Description = "x",
            RowVersion = new byte[] { 2, 0, 0, 0 },
            CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "u",
            LastModifiedBy = "u"
        };

        _cache.Setup(x => x.GetAsync<ProductDto>($"products:product:{id}")).ReturnsAsync((ProductDto?)null);
        _repo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);

        var service = CreateService();
        var result = await service.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        _cache.Verify(
            x => x.SetAsync($"products:product:{id}", It.IsAny<ProductDto>(), TimeSpan.FromMinutes(30)),
            Times.Once);
    }

    private ProductService CreateService()
    {
        return new ProductService(
            _repo.Object,
            _sequences.Object,
            _mapper,
            _logger.Object,
            _publisher.Object,
            _cache.Object);
    }
}
