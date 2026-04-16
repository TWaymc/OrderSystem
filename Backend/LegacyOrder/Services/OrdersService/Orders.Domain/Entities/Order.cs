namespace Orders.Domain.Entities;

// TODO: this should be a table in the database, so far I keep it simple
public enum OrderStatus
{
    Pending,
    Cancelled,
    Processed
}

public class Order : BaseEntity
{
    public Order()
    {
        OrderItems = new List<OrderItem>();
    }

    public Order(List<OrderItem> orderItems)
    {
        OrderItems = orderItems ?? new List<OrderItem>();
    }

    public Guid Id { get; set; }
    
    public string Code { get; set; } = string.Empty;
    
    public OrderStatus StatusCode { get; set; } = OrderStatus.Pending;
    
    // customer info
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerSurname { get; set; } = string.Empty;
    public string? CustomerMobileNumber { get; set; }
    public string? CustomerEmail { get; set; }
    
    public List<OrderItem> OrderItems { get; set; } = new();
    
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    
    public byte[] RowVersion { get; set; }
}