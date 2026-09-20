using InventoryService.Entities;

namespace InventoryService.Repository.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid productId);

    Task<Product?> GetByNameAsync(string productName);

    Task<List<Product>> GetPagedAsync(int pageNumber, int pageSize);

    Task<int> GetCountAsync();

    Task<Product> CreateAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(Product product);

    Task SaveChangesAsync();
    Task<bool> ReduceStockAsync(Guid productId, int quantity);
    Task<bool> RestoreStockAsync(Guid productId, int quantity);
}