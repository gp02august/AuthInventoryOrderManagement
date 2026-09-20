using System.ComponentModel.DataAnnotations;

namespace OrderService.DTO;

public class OrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}