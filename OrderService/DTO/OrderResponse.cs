namespace OrderService.DTO;

public class OrderResponse
{
    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public string OrderStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<OrderItemResponse> Items { get; set; } = new();
}