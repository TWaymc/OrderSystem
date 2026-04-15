namespace Orders.Application.DTOs;

public record ContactDto(
    Guid Id,
    string Code,
    string Name,
    string Surname,
    string? MobileNumber,
    string? Email);
