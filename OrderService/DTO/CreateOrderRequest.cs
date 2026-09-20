using System.ComponentModel.DataAnnotations;

namespace OrderService.DTO;

public class CreateOrderRequest
{
    [Required]
    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = new();
}