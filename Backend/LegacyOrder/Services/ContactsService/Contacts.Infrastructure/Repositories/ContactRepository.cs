using Contacts.Domain.Entities;
using Contacts.Domain.Interfaces;
using Contacts.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly AppDbContext _context;

    public ContactRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contact>> GetAllAsync()
    {
        return await _context.Contacts.ToListAsync();
    }

    public async Task<Contact?> GetByIdAsync(Guid id)
    {
        return await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contact> AddAsync(Contact contact)
    {
        contact.ModifiedAt = null;

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
        contact.ModifiedAt = DateTime.UtcNow;

        _context.Contacts.Update(contact);
        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task<bool> DeleteAsync(Guid id, string lastModifiedBy)
    {
        var contact = await _context.Contacts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);
        if (contact == null || contact.IsDeleted)
            return false;

        contact.IsDeleted = true;
        contact.ModifiedAt = DateTime.UtcNow;
        contact.LastModifiedBy = lastModifiedBy;
        await _context.SaveChangesAsync();
        return true;
    }
}
