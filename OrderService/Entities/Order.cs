namespace OrderService.Entities;

public class Order
{
    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public string OrderStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();
}