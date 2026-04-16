using AutoMapper;
using LoggingLib.Services.Interfaces;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Interfaces;
using Orders.Infrastructure.Messaging.Publisher.Interface;
using RedisCache.Service;

namespace Orders.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ISequenceService _sequences;
    private readonly IMapper _mapper;
    private readonly ILogPublisher _logger;
    private readonly IOrderPublisher _orderPublisher;
    private readonly IRedisCacheService _cache;
    private readonly IContactApiClient _contactApiClient;
    private readonly IProductApiClient _productApiClient;

    public OrderService(
        IOrderRepository repo,
        IOrderItemRepository orderItemRepository,
        ISequenceService sequences,
        IMapper mapper,
        ILogPublisher logger,
        IOrderPublisher orderPublisher,
        IRedisCacheService cache,
        IContactApiClient contactApiClient,
        IProductApiClient productApiClient)
    {
        _repo = repo;
        _orderItemRepository = orderItemRepository;
        _sequences = sequences;
        _mapper = mapper;
        _logger = logger;
        _orderPublisher = orderPublisher;
        _cache = cache;
        _contactApiClient = contactApiClient;
        _productApiClient = productApiClient;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var orderDto = await _cache.GetAsync<OrderDto>($"orders:order:{id}");

        if (orderDto != null)
            return orderDto;

        var order = await _repo.GetByIdAsync(id);
        if (order == null) return null;

        var result = _mapper.Map<OrderDto>(order);
        await _cache.SetAsync($"orders:order:{id}", result, TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, string createdBy)
    {
        var contact = await GetContactAsync(dto.CustomerId)
                      ?? throw new Exception($"Contact '{dto.CustomerId}' not found.");

        var code = await _sequences.GetNextOrderCodeAsync();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = code,
            StatusCode = OrderStatus.Pending,
            CustomerId = dto.CustomerId,
            CustomerName = contact.Name,
            CustomerSurname = contact.Surname,
            CustomerMobileNumber = contact.MobileNumber,
            CustomerEmail = contact.Email,
            CreatedBy = createdBy,
            LastModifiedBy = createdBy,
            OrderItems = new List<OrderItem>()
        };

        foreach (var item in dto.OrderItems ?? Array.Empty<CreateOrderItemDto>())
        {
            var product = await GetProductAsync(item.ProductId)
                          ?? throw new Exception($"Product '{item.ProductId}' not found.");

            order.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductCode = product.Code,
                ProductName = product.Name,
                ProductUnitPrice = product.Price,
                Quantity = item.Quantity,
                CreatedBy = createdBy,
                LastModifiedBy = createdBy
            });
        }

        RecalculateTotals(order);
        var created = await _repo.AddAsync(order);
        await _logger.InfoAsync($"New Order: {code} created by: {createdBy}");

        return _mapper.Map<OrderDto>(created);
    }

    public async Task<OrderDto> UpdateAsync(Guid id, UpdateOrderDto dto, string lastModifiedBy)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order == null)
            throw new Exception("Order not found");

        var contact = await GetContactAsync(dto.CustomerId)
                      ?? throw new Exception($"Contact '{dto.CustomerId}' not found.");

        order.CustomerId = dto.CustomerId;
        order.CustomerName = contact.Name;
        order.CustomerSurname = contact.Surname;
        order.CustomerMobileNumber = contact.MobileNumber;
        order.CustomerEmail = contact.Email;
        order.LastModifiedBy = lastModifiedBy;

        // RecalculateTotals(order); here is useless
        var updated = await _repo.UpdateAsync(order);
        await _cache.RemoveAsync($"orders:order:{id}");
        await _logger.InfoAsync($"Order: {order.Code} updated by: {lastModifiedBy}");

        return _mapper.Map<OrderDto>(updated);
    }

    public async Task<OrderDto> AddOrderItemAsync(Guid orderId, AddOrderItemDto dto, string lastModifiedBy)
    {
        var product = await GetProductAsync(dto.ProductId)
                      ?? throw new Exception($"Product '{dto.ProductId}' not found.");

        var existingItem = await _orderItemRepository.GetByOrderAndProductAsync(orderId, dto.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            existingItem.LastModifiedBy = lastModifiedBy;
            await _orderItemRepository.UpdateAsync(existingItem);
        }
        else
        {
            await _orderItemRepository.AddAsync(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = dto.ProductId,
                ProductCode = product.Code,
                ProductName = product.Name,
                ProductUnitPrice = product.Price,
                Quantity = dto.Quantity,
                CreatedBy = lastModifiedBy,
                LastModifiedBy = lastModifiedBy
            });
        }

        var order = await _repo.GetByIdAsync(orderId);
        if (order == null)
            throw new Exception("Order not found");

        order.LastModifiedBy = lastModifiedBy;

        RecalculateTotals(order);
        var updated = await _repo.UpdateAsync(order);
        await _cache.RemoveAsync($"orders:order:{orderId}");
        await _logger.InfoAsync($"Order item added to order: {order.Code} by: {lastModifiedBy}");

        return _mapper.Map<OrderDto>(updated);
    }

    public async Task<bool> RemoveOrderItemAsync(Guid orderId, Guid orderItemId, string lastModifiedBy)
    {
        var order = await _repo.GetByIdAsync(orderId);
        if (order == null)
            throw new Exception("Order not found");

        var item = order.OrderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null)
            return false;

        item.IsDeleted = true;
        item.ModifiedAt = DateTime.UtcNow;
        item.LastModifiedBy = lastModifiedBy;
        order.LastModifiedBy = lastModifiedBy;

        RecalculateTotals(order);
        await _repo.UpdateAsync(order);
        await _cache.RemoveAsync($"orders:order:{orderId}");
        await _logger.InfoAsync($"Order item removed from order: {order.Code} by: {lastModifiedBy}");

        return true;
    }

    private async Task<ContactDto?> GetContactAsync(Guid contactId)
    {
        var cacheKey = $"orders:contact:{contactId}";  // separated cache from the oen used on Contacts
        var cachedContact = await _cache.GetAsync<ContactDto>(cacheKey);
        if (cachedContact != null)
            return cachedContact;

        var contact = await _contactApiClient.GetByIdAsync(contactId);
        if (contact != null)
            await _cache.SetAsync(cacheKey, contact, TimeSpan.FromMinutes(30));

        return contact;
    }

    private async Task<ProductDto?> GetProductAsync(Guid productId)
    {
        var cacheKey = $"orders:product:{productId}";  // separated cache from the oen used on Products
        var cachedProduct = await _cache.GetAsync<ProductDto>(cacheKey);
        if (cachedProduct != null)
            return cachedProduct;

        var product = await _productApiClient.GetByIdAsync(productId);
        if (product != null)
            await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(30));

        return product;
    }

    private static void RecalculateTotals(Order order)
    {
        var activeItems = order.OrderItems.Where(i => !i.IsDeleted).ToList();
        order.TotalItems = activeItems.Sum(i => i.Quantity);
        order.TotalAmount = activeItems.Sum(i => i.ProductUnitPrice * i.Quantity);
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto, string lastModifiedBy)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order == null)
            throw new Exception("Order not found");

        order.StatusCode = dto.StatusCode;
        order.LastModifiedBy = lastModifiedBy;

        // RecalculateTotals(order); here is useless
        var updated = await _repo.UpdateAsync(order);

        // await _orderPublisher.PublishAsync(id);  Implemented but not consumed by anything in the system so far,  So Commented
        await _cache.RemoveAsync($"orders:order:{id}");
        await _logger.InfoAsync($"Order: {order.Code} status changed to {order.StatusCode} by: {lastModifiedBy}");

        return _mapper.Map<OrderDto>(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, string lastModifiedBy)
    {
        var res = await _repo.DeleteAsync(id, lastModifiedBy);

        if (res)
        {
            await _cache.RemoveAsync($"orders:order:{id}");
            // await _orderPublisher.PublishAsync(id);   Implemented but not consumed by anything in the system so far,  So Commented
        }

        return res;
    }
}
