namespace Orders.Domain.Entities;

public class OrderItem: BaseEntity
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;
    
    // products info
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductUnitPrice { get; set; }
    
    
    public int Quantity { get; set; }

}