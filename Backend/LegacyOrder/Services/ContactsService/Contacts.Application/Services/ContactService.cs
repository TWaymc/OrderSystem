using AutoMapper;
using Contacts.Application.DTOs;
using Contacts.Application.Interfaces;
using Contacts.Domain.Entities;
using Contacts.Domain.Interfaces;
using Contacts.Infrastructure.Messaging.Publisher.Interface;
using LoggingLib.Services.Interfaces;
using RedisCache.Service;

namespace Contacts.Application.Services;

public class ContactService : IContactService
{
    private readonly IContactRepository _repo;
    private readonly ISequenceService _sequences;
    private readonly IMapper _mapper;
    private readonly ILogPublisher _logger;
    private readonly IContactPublisher _contactPublisher;
    private readonly IRedisCacheService _cache;

    public ContactService(
        IContactRepository repo,
        ISequenceService sequences,
        IMapper mapper,
        ILogPublisher logger,
        IContactPublisher contactPublisher,
        IRedisCacheService cache)
    {
        _repo = repo;
        _sequences = sequences;
        _mapper = mapper;
        _logger = logger;
        _contactPublisher = contactPublisher;
        _cache = cache;
    }

    public async Task<IEnumerable<ContactDto>> GetAllAsync()
    {
        var contacts = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<ContactDto>>(contacts);
    }

    public async Task<ContactDto?> GetByIdAsync(Guid id)
    {
        var contactDto = await _cache.GetAsync<ContactDto>($"contacts:contact:{id}");

        if (contactDto != null)
            return contactDto;

        var contact = await _repo.GetByIdAsync(id);
        if (contact == null) return null;

        var result = _mapper.Map<ContactDto>(contact);
        await _cache.SetAsync($"contacts:contact:{id}", result, TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<ContactDto> CreateAsync(CreateContactDto dto, string createdBy)
    {
        var code = await _sequences.GetNextContactCodeAsync();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = dto.Name,
            Surname = dto.Surname,
            MobileNumber = dto.MobileNumber,
            Email = dto.Email,
            CreatedBy = createdBy,
            LastModifiedBy = createdBy
        };

        var created = await _repo.AddAsync(contact);
        await _logger.InfoAsync($"New Contact: {code} created by: {createdBy}");

        return _mapper.Map<ContactDto>(created);
    }

    public async Task<ContactDto> UpdateAsync(Guid id, UpdateContactDto dto, string lastModifiedBy)
    {
        var contact = await _repo.GetByIdAsync(id);
        if (contact == null)
            throw new Exception("Contact not found");

        contact.Name = dto.Name;
        contact.Surname = dto.Surname;
        contact.MobileNumber = dto.MobileNumber;
        contact.Email = dto.Email;
        contact.LastModifiedBy = lastModifiedBy;

        var updated = await _repo.UpdateAsync(contact);

        await _contactPublisher.PublishAsync(id);
        await _cache.RemoveAsync($"contacts:contact:{id}");
        await _logger.InfoAsync($"Contact: {contact.Code} modified by: {lastModifiedBy}");

        return _mapper.Map<ContactDto>(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, string lastModifiedBy)
    {
        var res = await _repo.DeleteAsync(id, lastModifiedBy);

        if (res)
        {
            await _cache.RemoveAsync($"contacts:contact:{id}");
            await _contactPublisher.PublishAsync(id);
        }

        return res;
    }
}
