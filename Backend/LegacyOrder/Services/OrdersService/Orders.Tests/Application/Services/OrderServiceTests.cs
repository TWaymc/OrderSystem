using AutoMapper;
using FluentAssertions;
using LoggingLib.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Orders.Application.Mappings;
using Orders.Application.Services;
using Orders.Domain.Entities;
using Orders.Domain.Interfaces;
using Orders.Infrastructure.Messaging.Publisher.Interface;
using RedisCache.Service;

namespace Orders.Tests.Application.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IOrderItemRepository> _orderItemRepository = new();
    private readonly Mock<ISequenceService> _sequenceService = new();
    private readonly Mock<ILogPublisher> _logPublisher = new();
    private readonly Mock<IOrderPublisher> _orderPublisher = new();
    private readonly Mock<IRedisCacheService> _cacheService = new();
    private readonly Mock<IContactApiClient> _contactApiClient = new();
    private readonly Mock<IProductApiClient> _productApiClient = new();
    private readonly IMapper _mapper;

    public OrderServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>(), loggerFactory);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_ShouldCalculateTotalsAndReturnCreatedOrder()
    {
        // Arrange
        _sequenceService.Setup(x => x.GetNextOrderCodeAsync(It.IsAny<CancellationToken>())).ReturnsAsync("ORD-0001");
        _cacheService.Setup(x => x.GetAsync<ContactDto>(It.IsAny<string>())).ReturnsAsync((ContactDto?)null);
        _cacheService.Setup(x => x.GetAsync<ProductDto>(It.IsAny<string>())).ReturnsAsync((ProductDto?)null);
        _contactApiClient.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new ContactDto(Guid.NewGuid(), "C-001", "John", "Doe", "123", "john@doe.com"));
        _productApiClient.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new ProductDto(Guid.NewGuid(), "P-001", "Product", 10m));
        _orderRepository.Setup(x => x.AddAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);

        var service = CreateService();
        var dto = new CreateOrderDto(Guid.NewGuid(), new[]
        {
            new CreateOrderItemDto(Guid.NewGuid(), 2),
            new CreateOrderItemDto(Guid.NewGuid(), 1)
        });

        // Act
        var result = await service.CreateAsync(dto, "tester");

        // Assert
        result.TotalItems.Should().Be(3);
        result.TotalAmount.Should().Be(30m);
        result.RowVersion.Should().NotBeNull();
        _orderRepository.Verify(x => x.AddAsync(It.Is<Order>(o => o.TotalItems == 3 && o.TotalAmount == 30m)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPassOriginalRowVersionToRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var originalRowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        var entity = new Order
        {
            Id = id,
            Code = "ORD-0002",
            RowVersion = new byte[] { 5, 6, 7, 8 }
        };

        _orderRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);
        _cacheService.Setup(x => x.GetAsync<ContactDto>(It.IsAny<string>())).ReturnsAsync((ContactDto?)null);
        _contactApiClient.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new ContactDto(Guid.NewGuid(), "C-002", "Jane", "Doe", null, "jane@doe.com"));
        _orderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<byte[]?>()))
            .ReturnsAsync((Order o, byte[]? _) => o);

        var service = CreateService();
        var dto = new UpdateOrderDto(Guid.NewGuid(), originalRowVersion);

        // Act
        await service.UpdateAsync(id, dto, "tester");

        // Assert
        _orderRepository.Verify(
            x => x.UpdateAsync(
                It.Is<Order>(o => o.Id == id),
                It.Is<byte[]?>(v => v != null && v.SequenceEqual(new byte[] { 1, 2, 3, 4 }))),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldPassOriginalRowVersionToRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var originalRowVersion = Convert.ToBase64String(new byte[] { 9, 9, 9, 9 });
        var entity = new Order
        {
            Id = id,
            Code = "ORD-0003",
            RowVersion = new byte[] { 1, 1, 1, 1 },
            StatusCode = OrderStatus.Pending
        };

        _orderRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);
        _orderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<byte[]?>()))
            .ReturnsAsync((Order o, byte[]? _) => o);

        var service = CreateService();
        var dto = new UpdateOrderStatusDto(OrderStatus.Processed, originalRowVersion);

        // Act
        var result = await service.UpdateStatusAsync(id, dto, "tester");

        // Assert
        result.StatusCode.Should().Be(OrderStatus.Processed);
        _orderRepository.Verify(
            x => x.UpdateAsync(
                It.Is<Order>(o => o.StatusCode == OrderStatus.Processed),
                It.Is<byte[]?>(v => v != null && v.SequenceEqual(new byte[] { 9, 9, 9, 9 }))),
            Times.Once);
    }

    private OrderService CreateService()
    {
        return new OrderService(
            _orderRepository.Object,
            _orderItemRepository.Object,
            _sequenceService.Object,
            _mapper,
            _logPublisher.Object,
            _orderPublisher.Object,
            _cacheService.Object,
            _contactApiClient.Object,
            _productApiClient.Object);
    }
}
