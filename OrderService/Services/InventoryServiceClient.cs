using System.Net;
using System.Net.Http.Json;
using OrderService.DTO;
using OrderService.Services.Interfaces;

namespace OrderService.Services;

public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<InventoryServiceClient> _logger;

    public InventoryServiceClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<InventoryServiceClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private void AddAuthorizationHeader()
    {
        var authorizationHeader =
            _httpContextAccessor.HttpContext?
                .Request.Headers.Authorization
                .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            _httpClient.DefaultRequestHeaders.Remove(
                "Authorization");

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Authorization",
                authorizationHeader);
        }
    }

    public async Task<InventoryProductResponse?> GetProductAsync(
        Guid productId)
    {
        AddAuthorizationHeader();

        var response = await _httpClient.GetAsync(
            $"api/products/{productId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<InventoryProductResponse>();
    }

    public async Task<bool> ReduceStockAsync(
        Guid productId,
        int quantity)
    {
        AddAuthorizationHeader();

        var request = new ReduceStockRequest
        {
            Quantity = quantity
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"api/products/{productId}/reduce_stock",
            request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Failed to reduce stock for product {ProductId}. StatusCode: {StatusCode}",
                productId,
                response.StatusCode);

            return false;
        }

        return true;
    }

    public async Task<bool> RestoreStockAsync(
        Guid productId,
        int quantity)
    {
        AddAuthorizationHeader();

        var request = new RestoreStockRequest
        {
            Quantity = quantity
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"api/products/{productId}/restore_stock",
            request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Failed to restore stock for product {ProductId}. StatusCode: {StatusCode}",
                productId,
                response.StatusCode);

            return false;
        }

        return true;
    }
}