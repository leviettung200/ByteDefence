using ByteDefence.Shared.Models;

namespace ByteDefence.Api.Services;

public class NoOpNotificationService : INotificationService
{
    public Task BroadcastOrderCreated(Order order) => Task.CompletedTask;
    public Task BroadcastOrderUpdated(Order order) => Task.CompletedTask;
    public Task BroadcastOrderDeleted(string orderId) => Task.CompletedTask;
}
