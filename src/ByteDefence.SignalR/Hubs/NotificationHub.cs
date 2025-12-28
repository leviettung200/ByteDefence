using Microsoft.AspNetCore.SignalR;

namespace ByteDefence.SignalR.Hubs;

public class NotificationHub : Hub
{
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(orderId));
    }

    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, Group(orderId));
    }

    private static string Group(string id) => $"order-{id}";
}
