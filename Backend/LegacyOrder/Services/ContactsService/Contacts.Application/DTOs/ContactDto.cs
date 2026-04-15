namespace Contacts.Application.DTOs;

public record ContactDto(
    Guid Id,
    string Code,
    string Name,
    string Surname,
    string? MobileNumber,
    string? Email,
    string CreatedBy,
    string LastModifiedBy,
    DateTime CreatedAt,
    DateTime? ModifiedAt);

public record CreateContactDto(
    string Name,
    string Surname,
    string? MobileNumber,
    string? Email);

public record UpdateContactDto(
    string Name,
    string Surname,
    string? MobileNumber,
    string? Email);
