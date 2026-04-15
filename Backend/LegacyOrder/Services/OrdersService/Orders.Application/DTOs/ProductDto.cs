namespace Orders.Application.DTOs;

/// <summary>Read model: product fields plus audit timestamps from persistence.</summary>
public record ProductDto(
    Guid Id,
    string Code,
    string Name,
    decimal Price);

