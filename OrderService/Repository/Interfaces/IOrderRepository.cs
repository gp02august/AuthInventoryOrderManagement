using OrderService.Entities;

namespace OrderService.Repository.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId);

    Task<List<Order>> GetByUserIdAsync(Guid userId);

    Task<List<Order>> GetAllAsync();

    Task<Order> CreateAsync(Order order);

    Task UpdateAsync(Order order);

    Task SaveChangesAsync();
}