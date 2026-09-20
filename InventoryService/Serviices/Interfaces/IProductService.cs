using InventoryService.DTO;

namespace InventoryService.Services.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto?> GetByIdAsync(Guid productId);

    Task<ProductResponseDto?> GetByNameAsync(string productName);

    Task<PagedResponseDto<ProductResponseDto>> GetPagedAsync(
        int pageNumber,
        int pageSize);

    Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request);

    Task<bool> UpdateAsync(
        Guid productId,
        UpdateProductRequestDto request);

    Task<bool> DeleteAsync(Guid productId);
    Task<bool> ReduceStockAsync(Guid productId, int quantity);
    Task<bool> RestoreStockAsync(Guid productId, int quantity);
}