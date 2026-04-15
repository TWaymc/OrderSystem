namespace Contacts.Domain.Entities;

public class Contact : BaseEntity
{
    public Guid Id { get; set; }
    
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    
    
}