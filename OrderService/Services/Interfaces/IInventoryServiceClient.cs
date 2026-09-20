using OrderService.DTO;

namespace OrderService.Services.Interfaces;

public interface IInventoryServiceClient
{
    Task<InventoryProductResponse?> GetProductAsync(
        Guid productId);

    Task<bool> ReduceStockAsync(
        Guid productId,
        int quantity);

    Task<bool> RestoreStockAsync(
        Guid productId,
        int quantity);
}