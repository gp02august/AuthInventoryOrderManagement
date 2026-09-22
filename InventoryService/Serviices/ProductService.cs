using InventoryService.DTO;
using InventoryService.Entities;
using InventoryService.Repository.Interfaces;
using InventoryService.Services.Interfaces;

namespace InventoryService.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ProductResponseDto?> GetByIdAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
            return null;

        return MapToResponse(product);
    }

    public async Task<ProductResponseDto?> GetByNameAsync(string productName)
    {
        var product = await _productRepository.GetByNameAsync(productName);

        if (product == null)
            return null;

        return MapToResponse(product);
    }

    public async Task<PagedResponseDto<ProductResponseDto>> GetPagedAsync(
        int pageNumber,
        int pageSize)
    {
        var totalCount = await _productRepository.GetCountAsync();

        var products = await _productRepository
            .GetPagedAsync(pageNumber, pageSize);

        var totalPages = (int)Math.Ceiling(
            (double)totalCount / pageSize);

        return new PagedResponseDto<ProductResponseDto>
        {
            Items = products
                .Select(MapToResponse)
                .ToList(),

            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<ProductResponseDto> CreateAsync(
        CreateProductRequestDto request)
    {
        var existingProduct =
            await _productRepository.GetByNameAsync(
                request.ProductName);

        if (existingProduct != null)
        {
            throw new InvalidOperationException(
                "Product with the same name already exists.");
        }

        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            ProductName = request.ProductName,
            StockQty = request.StockQty,
            IsActive = true,
            CreatedAt = DateTime.SpecifyKind(
                DateTime.UtcNow,
                DateTimeKind.Unspecified),
            UpdatedAt = null
        };

        var createdProduct =
            await _productRepository.CreateAsync(product);

        _logger.LogInformation(
            "Product {ProductId} created successfully. ProductName: {ProductName}",
            createdProduct.ProductId,
            createdProduct.ProductName);

        return MapToResponse(createdProduct);
    }

    public async Task<bool> UpdateAsync(
        Guid productId,
        UpdateProductRequestDto request)
    {
        var product =
            await _productRepository.GetByIdAsync(productId);

        if (product == null)
            return false;

        var existingProduct =
            await _productRepository.GetByNameAsync(
                request.ProductName);

        if (existingProduct != null &&
            existingProduct.ProductId != productId)
        {
            throw new InvalidOperationException(
                "Another product with the same name already exists.");
        }

        product.ProductName = request.ProductName;
        product.StockQty = request.StockQty;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.SpecifyKind(
            DateTime.UtcNow,
            DateTimeKind.Unspecified);

        await _productRepository.UpdateAsync(product);

        _logger.LogInformation(
            "Product {ProductId} updated successfully.",
            productId);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid productId)
    {
        var product =
            await _productRepository.GetByIdAsync(productId);

        if (product == null)
            return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.SpecifyKind(
            DateTime.UtcNow,
            DateTimeKind.Unspecified);

        await _productRepository.UpdateAsync(product);

        _logger.LogInformation(
            "Product {ProductId} deactivated successfully.",
            productId);

        return true;
    }
    public async Task<bool> ReduceStockAsync(
    Guid productId,
    int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        if (!product.IsActive)
        {
            throw new InvalidOperationException(
                "Product is inactive.");
        }

        if (product.StockQty < quantity)
        {
            throw new InvalidOperationException(
                "Insufficient stock.");
        }

        return await _productRepository.ReduceStockAsync(
            productId,
            quantity);
    }
    public async Task<bool> RestoreStockAsync(
    Guid productId,
    int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var product = await _productRepository
            .GetByIdAsync(productId);

        if (product == null)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        if (!product.IsActive)
        {
            throw new InvalidOperationException(
                "Product is inactive.");
        }

        return await _productRepository
            .RestoreStockAsync(productId, quantity);
    }
    private static ProductResponseDto MapToResponse(Product product)
    {
        return new ProductResponseDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            StockQty = product.StockQty,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}