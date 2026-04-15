namespace Products.Domain.Entities;

public class Product : BaseEntity
{
    public Guid Id { get; set; }
    
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    
    
}