using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTO;
using OrderService.Services.Interfaces;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // POST: api/orders
    [HttpPost]
    [Authorize(Roles = "USER")]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request)
    {
        var userId = GetUserId();

        var order = await _orderService.CreateOrderAsync(
            userId,
            request);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = order.OrderId },
            order);
    }

    // GET: api/orders/my-orders
    [HttpGet("my-orders")]
    [Authorize(Roles = "USER")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetUserId();

        var orders = await _orderService
            .GetMyOrdersAsync(userId);

        return Ok(orders);
    }

    // GET: api/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(
        Guid id)
    {
        var userId = GetUserId();

        var isAdmin = User.IsInRole("ADMIN");

        var order = await _orderService.GetOrderByIdAsync(
            id,
            userId,
            isAdmin);

        if (order == null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        return Ok(order);
    }

    // PATCH: api/orders/{id}/cancel
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(
        Guid id)
    {
        var userId = GetUserId();

        var isAdmin = User.IsInRole("ADMIN");

        var order = await _orderService.CancelOrderAsync(
            id,
            userId,
            isAdmin);

        return Ok(order);
    }

    private Guid GetUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user identity.");
        }

        return userId;
    }
}