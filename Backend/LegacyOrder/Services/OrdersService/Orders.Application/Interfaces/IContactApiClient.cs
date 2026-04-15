using Orders.Application.DTOs;

namespace Orders.Application.Interfaces;

public interface IContactApiClient
{
    Task<ContactDto?> GetByIdAsync(Guid id);
}