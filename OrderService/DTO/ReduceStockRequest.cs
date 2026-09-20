using System.ComponentModel.DataAnnotations;

namespace OrderService.DTO;

public class ReduceStockRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}