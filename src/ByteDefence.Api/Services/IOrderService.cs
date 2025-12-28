using ByteDefence.Shared.DTOs;
using ByteDefence.Shared.Models;

namespace ByteDefence.Api.Services;

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken = default);
    Task<OrderStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    Task<Order> CreateOrderAsync(CreateOrderInput input, User currentUser, CancellationToken cancellationToken = default);
    Task<Order> UpdateOrderAsync(UpdateOrderInput input, User currentUser, CancellationToken cancellationToken = default);
    Task<bool> DeleteOrderAsync(string id, CancellationToken cancellationToken = default);
    Task<Order> AddOrderItemAsync(AddOrderItemInput input, User currentUser, CancellationToken cancellationToken = default);
}
