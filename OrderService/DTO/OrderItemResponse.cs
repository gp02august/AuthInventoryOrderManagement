namespace OrderService.DTO;

public class OrderItemResponse
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}