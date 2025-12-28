using ByteDefence.Shared.Models;

namespace ByteDefence.Api.Services;

public interface INotificationService
{
    Task BroadcastOrderCreated(Order order);
    Task BroadcastOrderUpdated(Order order);
    Task BroadcastOrderDeleted(string orderId);
}
