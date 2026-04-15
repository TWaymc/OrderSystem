using Contacts.Domain.Entities;

namespace Contacts.Domain.Interfaces;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id);
    Task<IEnumerable<Contact>> GetAllAsync();
    Task<Contact> AddAsync(Contact contact);
    Task<Contact> UpdateAsync(Contact contact);
    Task<bool> DeleteAsync(Guid id, string lastUpdatedBy);
}
