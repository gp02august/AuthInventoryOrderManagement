using System.ComponentModel.DataAnnotations;

namespace InventoryService.DTO;

public class CreateProductRequestDto
{
    [Required]
    [StringLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQty { get; set; }
}