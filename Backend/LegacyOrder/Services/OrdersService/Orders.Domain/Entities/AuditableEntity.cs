namespace Orders.Domain.Entities;

/// <summary>
/// Base type for persistence: audit fields and soft delete.
/// Use a class (not an interface) so these properties are declared once and mapped by EF like any other columns.
/// </summary>
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    
    public string LastModifiedBy { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}
