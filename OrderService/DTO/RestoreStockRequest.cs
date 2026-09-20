using System.ComponentModel.DataAnnotations;

namespace OrderService.DTO;

public class RestoreStockRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}