using InventoryService.DTO;
using InventoryService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetById(Guid productId)
    {
        var product = await _productService.GetByIdAsync(productId);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        return Ok(product);
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> GetByName(
        [FromQuery] string productName)
    {
        var product =
            await _productService.GetByNameAsync(productName);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest(
                "Page number and page size must be greater than zero.");

        var result =
            await _productService.GetPagedAsync(
                pageNumber,
                pageSize);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequestDto request)
    {
        try
        {
            var product =
                await _productService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { productId = product.ProductId },
                product);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{productId:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(
        Guid productId,
        [FromBody] UpdateProductRequestDto request)
    {
        try
        {
            var updated =
                await _productService.UpdateAsync(
                    productId,
                    request);

            if (!updated)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("{productId:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(Guid productId)
    {
        var deleted =
            await _productService.DeleteAsync(productId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
    [HttpPost("{productId:guid}/reduce_stock")]
    [Authorize]
    public async Task<IActionResult> ReduceStock(
    Guid productId,
    [FromBody] ReduceStockRequestDto request)
    {
        try
        {
            var result = await _productService.ReduceStockAsync(
                productId,
                request.Quantity);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Unable to reduce stock."
                });
            }

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
    [HttpPost("{productId}/restore_stock")]
    [Authorize]
    public async Task<IActionResult> RestoreStock(
    Guid productId,
    [FromBody] RestoreStockRequest request)
    {
        var result = await _productService.RestoreStockAsync(
            productId,
            request.Quantity);

        if (!result)
        {
            return BadRequest(new
            {
                message = "Stock could not be restored."
            });
        }

        return Ok(new
        {
            message = "Stock restored successfully."
        });
    }
}