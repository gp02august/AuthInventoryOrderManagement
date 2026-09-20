using InventoryService.Data;
using InventoryService.Entities;
using InventoryService.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repository;

public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _context;

    public ProductRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid productId)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x => x.ProductId == productId);
    }

    public async Task<Product?> GetByNameAsync(string productName)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x => x.ProductName == productName);
    }

    public async Task<List<Product>> GetPagedAsync(
        int pageNumber,
        int pageSize)
    {
        return await _context.Products
            .OrderBy(x => x.ProductName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Products.CountAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<bool> ReduceStockAsync(Guid productId, int quantity)
    {
        var rowsAffected = await _context.Products
            .Where(p =>
                p.ProductId == productId &&
                p.IsActive &&
                p.StockQty >= quantity)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    p => p.StockQty,
                    p => p.StockQty - quantity));

        return rowsAffected > 0;
    }
    public async Task<bool> RestoreStockAsync(
    Guid productId,
    int quantity)
    {
        var rowsAffected = await _context.Products
            .Where(p =>
                p.ProductId == productId &&
                p.IsActive)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    p => p.StockQty,
                    p => p.StockQty + quantity));

        return rowsAffected > 0;
    }
}