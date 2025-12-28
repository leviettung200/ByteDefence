using Microsoft.AspNetCore.SignalR.Client;

namespace ByteDefence.Web.Services;

public class SignalRService : IAsyncDisposable
{
    private readonly AuthService _authService;
    public HubConnection Connection { get; }

    public SignalRService(IConfiguration configuration, AuthService authService)
    {
        _authService = authService;
        var hubUrl = configuration["SignalR:HubUrl"] ?? "http://localhost:5000/hubs/notifications";

        Connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = authService.GetTokenAsync;
            })
            .WithAutomaticReconnect()
            .Build();
    }

    public Task StartAsync() => Connection.StartAsync();

    public Task StopAsync() => Connection.StopAsync();

    public async Task JoinOrderGroupAsync(string orderId)
    {
        if (Connection.State == HubConnectionState.Disconnected)
        {
            await StartAsync();
        }

        await Connection.InvokeAsync("JoinOrderGroup", orderId);
    }

    public async Task LeaveOrderGroupAsync(string orderId)
    {
        if (Connection.State == HubConnectionState.Connected)
        {
            await Connection.InvokeAsync("LeaveOrderGroup", orderId);
        }
    }

    public ValueTask DisposeAsync()
    {
        return Connection.DisposeAsync();
    }
}
