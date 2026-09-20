using OrderService.DTO;

namespace OrderService.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request);

    Task<List<OrderResponse>> GetMyOrdersAsync(
        Guid userId);

    Task<OrderResponse?> GetOrderByIdAsync(
        Guid orderId,
        Guid userId,
        bool isAdmin);

    Task<OrderResponse> CancelOrderAsync(
        Guid orderId,
        Guid userId,
        bool isAdmin);
}