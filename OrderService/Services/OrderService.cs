using OrderService.DTO;
using OrderService.Entities;
using OrderService.Repository.Interfaces;
using OrderService.Services.Interfaces;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IInventoryServiceClient inventoryServiceClient,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _inventoryServiceClient = inventoryServiceClient;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request)
    {
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Invalid user.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException(
                "Order must contain at least one item.");
        }

        // Aggregate duplicate products.
        var requestedItems = request.Items
            .GroupBy(x => x.ProductId)
            .Select(group => new OrderItemRequest
            {
                ProductId = group.Key,
                Quantity = group.Sum(x => x.Quantity)
            })
            .ToList();

        // Validate quantities.
        foreach (var item in requestedItems)
        {
            if (item.ProductId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Product ID cannot be empty.");
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }
        }

        // ---------------------------------------------------------
        // Step 1: Validate all products before reducing any stock.
        // ---------------------------------------------------------

        var products = new Dictionary<Guid, InventoryProductResponse>();

        foreach (var item in requestedItems)
        {
            var product =
                await _inventoryServiceClient
                    .GetProductAsync(item.ProductId);

            if (product == null)
            {
                throw new KeyNotFoundException(
                    $"Product {item.ProductId} not found.");
            }

            if (!product.IsActive)
            {
                throw new InvalidOperationException(
                    $"Product '{product.ProductName}' is inactive.");
            }

            if (product.StockQty < item.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.ProductName}'.");
            }

            products[item.ProductId] = product;
        }

        // ---------------------------------------------------------
        // Step 2: Reduce stock.
        //
        // If one reduction fails after previous reductions succeeded,
        // restore the already reduced stock.
        // ---------------------------------------------------------

        var reducedItems = new List<OrderItemRequest>();

        try
        {
            foreach (var item in requestedItems)
            {
                var reduced =
                    await _inventoryServiceClient
                        .ReduceStockAsync(
                            item.ProductId,
                            item.Quantity);

                if (!reduced)
                {
                    throw new InvalidOperationException(
                        $"Unable to reduce stock for product " +
                        $"'{products[item.ProductId].ProductName}'.");
                }

                reducedItems.Add(item);
            }

            // -----------------------------------------------------
            // Step 3: Create order in Order DB.
            // -----------------------------------------------------

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = userId,
                OrderStatus = "CREATED",
                CreatedAt = DateTime.SpecifyKind(
                    DateTime.UtcNow,
                    DateTimeKind.Unspecified)
            };

            foreach (var item in requestedItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            await _orderRepository.CreateAsync(order);

            await _orderRepository.SaveChangesAsync();

            // -----------------------------------------------------
            // Step 4: Mark order as CONFIRMED.
            // -----------------------------------------------------

            order.OrderStatus = "CONFIRMED";

            await _orderRepository.UpdateAsync(order);

            await _orderRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Order {OrderId} created successfully for user {UserId}.",
                order.OrderId,
                userId);

            return MapToResponse(order);
        }
        catch
        {
            // -----------------------------------------------------
            // Compensation:
            // If anything fails after stock was reduced,
            // restore all previously reduced stock.
            // -----------------------------------------------------

            foreach (var item in reducedItems)
            {
                try
                {
                    await _inventoryServiceClient
                        .RestoreStockAsync(
                            item.ProductId,
                            item.Quantity);
                }
                catch (Exception compensationException)
                {
                    _logger.LogError(
                        compensationException,
                        "Failed to restore stock for product {ProductId} " +
                        "during order compensation.",
                        item.ProductId);
                }
            }

            throw;
        }
    }

    public async Task<List<OrderResponse>> GetMyOrdersAsync(
        Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Invalid user.");
        }

        var orders =
            await _orderRepository.GetByUserIdAsync(userId);

        return orders
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(
        Guid orderId,
        Guid userId,
        bool isAdmin)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Invalid order ID.");
        }

        var order =
            await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            return null;
        }

        // Admin can access any order.
        if (isAdmin)
        {
            return MapToResponse(order);
        }

        // Normal user can access only own order.
        if (order.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to access this order.");
        }

        return MapToResponse(order);
    }

    public async Task<OrderResponse> CancelOrderAsync(
        Guid orderId,
        Guid userId,
        bool isAdmin)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Invalid order ID.");
        }

        var order =
            await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        // Normal user can cancel only own order.
        if (!isAdmin && order.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to cancel this order.");
        }

        if (order.OrderStatus == "CANCELLED")
        {
            throw new InvalidOperationException(
                "Order is already cancelled.");
        }

        if (order.OrderStatus != "CONFIRMED")
        {
            throw new InvalidOperationException(
                "Only confirmed orders can be cancelled.");
        }

        // Restore stock for every order item.
        foreach (var item in order.OrderItems)
        {
            var restored =
                await _inventoryServiceClient
                    .RestoreStockAsync(
                        item.ProductId,
                        item.Quantity);

            if (!restored)
            {
                throw new InvalidOperationException(
                    $"Unable to restore stock for product " +
                    $"{item.ProductId}.");
            }
        }

        order.OrderStatus = "CANCELLED";

        await _orderRepository.UpdateAsync(order);

        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Order {OrderId} cancelled by user {UserId}.",
            orderId,
            userId);

        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(
        Order order)
    {
        return new OrderResponse
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,

            Items = order.OrderItems
                .Select(item => new OrderItemResponse
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };
    }
}