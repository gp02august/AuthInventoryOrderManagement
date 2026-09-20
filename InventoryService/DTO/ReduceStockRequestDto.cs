using System.ComponentModel.DataAnnotations;

namespace InventoryService.DTO
{
    public class ReduceStockRequestDto
    {
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
