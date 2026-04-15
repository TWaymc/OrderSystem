namespace Products.Application.DTOs;

/// <summary>Read model: product fields plus audit timestamps from persistence.</summary>
public record ProductDto(
    Guid Id,
    string Code,
    string Name,
    decimal Price,
    string Description,
    string CreatedBy,
    string LastModifiedBy,
    DateTime CreatedAt,
    DateTime? ModifiedAt);

public record CreateProductDto(
    string Name,
    decimal Price,
    string Description);

public record UpdateProductDto(
    string Name,
    decimal Price,
    string Description);
