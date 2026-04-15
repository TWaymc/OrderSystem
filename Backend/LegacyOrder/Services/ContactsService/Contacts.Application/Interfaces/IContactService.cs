using Contacts.Application.DTOs;

namespace Contacts.Application.Interfaces;

public interface IContactService
{
    Task<IEnumerable<ContactDto>> GetAllAsync();

    Task<ContactDto?> GetByIdAsync(Guid id);

    Task<ContactDto> CreateAsync(CreateContactDto contactDto, string createdBy);

    Task<ContactDto> UpdateAsync(Guid id, UpdateContactDto dto, string lastModifiedBy);

    Task<bool> DeleteAsync(Guid id, string lastModifiedBy);
}
