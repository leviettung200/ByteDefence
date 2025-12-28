using System.Net.Http.Json;
using ByteDefence.Api.Options;
using ByteDefence.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ByteDefence.Api.Services;

public class LocalNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocalNotificationService> _logger;
    private readonly SignalROptions _options;

    public LocalNotificationService(HttpClient httpClient, IOptions<SignalROptions> options, ILogger<LocalNotificationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_options.HubUrl.TrimEnd('/'));
    }

    public Task BroadcastOrderCreated(Order order) => Send("OrderCreated", Group(order.Id), order);

    public Task BroadcastOrderUpdated(Order order) => Send("OrderUpdated", Group(order.Id), order);

    public Task BroadcastOrderDeleted(string orderId) => Send("OrderDeleted", Group(orderId), new { id = orderId });

    private async Task Send(string method, string group, object payload)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/broadcast", new
            {
                method,
                group,
                data = payload
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SignalR broadcast {Method} failed with {Status}", method, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR broadcast {Method} failed", method);
        }
    }

    private static string Group(string id) => $"order-{id}";
}
