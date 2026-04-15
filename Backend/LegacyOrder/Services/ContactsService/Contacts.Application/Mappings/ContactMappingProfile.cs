using AutoMapper;
using Contacts.Application.DTOs;
using Contacts.Domain.Entities;

namespace Contacts.Application.Mappings;

public class ContactMappingProfile : Profile
{
    public ContactMappingProfile()
    {
        CreateMap<Contact, ContactDto>();
    }
}
