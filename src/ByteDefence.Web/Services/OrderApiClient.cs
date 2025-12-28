using System.Net.Http.Headers;
using ByteDefence.Shared.DTOs;
using ByteDefence.Shared.Models;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace ByteDefence.Web.Services;

public class OrderApiClient
{
    private readonly GraphQLHttpClient _client;
    private readonly AuthService _auth;

    private const string OrderFields = "id title status createdAt updatedAt total createdBy { id displayName username role } items { id name quantity price lineTotal }";

    public OrderApiClient(GraphQLHttpClient client, AuthService auth)
    {
        _client = client;
        _auth = auth;
    }

    public async Task<(IReadOnlyList<Order> Orders, OrderStatistics Stats)> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthAsync();
        var request = new GraphQLRequest
        {
            Query = $"query OrdersQuery {{ orders {{ {OrderFields} }} orderStats {{ draft pending approved completed cancelled total }} }}"
        };

        var response = await _client.SendQueryAsync<OrdersResponse>(request, cancellationToken);
        return (response.Data?.Orders ?? new List<Order>(), response.Data?.OrderStats ?? new OrderStatistics());
    }

    public async Task<Order?> GetOrderAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureAuthAsync();
        var request = new GraphQLRequest
        {
            Query = $"query GetOrder($id: ID!) {{ order(id: $id) {{ {OrderFields} }} }}",
            Variables = new { id }
        };

        var response = await _client.SendQueryAsync<OrderResponse>(request, cancellationToken);
        return response.Data?.Order;
    }

    public async Task<Order?> CreateOrderAsync(CreateOrderInput input, CancellationToken cancellationToken = default)
    {
        await EnsureAuthAsync();
        var request = new GraphQLRequest
        {
            Query = $"mutation CreateOrder($input: CreateOrderInput!) {{ createOrder(input: $input) {{ {OrderFields} }} }}",
            Variables = new { input }
        };

        var response = await _client.SendMutationAsync<OrderResponse>(request, cancellationToken);
        return response.Data?.CreateOrder;
    }

    public async Task<Order?> UpdateOrderAsync(UpdateOrderInput input, CancellationToken cancellationToken = default)
    {
        await EnsureAuthAsync();
        var request = new GraphQLRequest
        {
            Query = $"mutation UpdateOrder($input: UpdateOrderInput!) {{ updateOrder(input: $input) {{ {OrderFields} }} }}",
            Variables = new { input }
        };

        var response = await _client.SendMutationAsync<OrderResponse>(request, cancellationToken);
        return response.Data?.UpdateOrder;
    }

    public async Task<bool> DeleteOrderAsync(string id, CancellationToken cancellationToken = default)
    {
        await EnsureAuthAsync();
        var request = new GraphQLRequest
        {
            Query = "mutation DeleteOrder($id: ID!) { deleteOrder(id: $id) }",
            Variables = new { id }
        };

        var response = await _client.SendMutationAsync<DeleteOrderResponse>(request, cancellationToken);
        return response.Data?.DeleteOrder ?? false;
    }

    public async Task<Order?> AddOrderItemAsync(AddOrderItemInput input, CancellationToken cancellationToken = default)
    {
        await EnsureAuthAsync();
        var request = new GraphQLRequest
        {
            Query = $"mutation AddOrderItem($input: AddOrderItemInput!) {{ addOrderItem(input: $input) {{ {OrderFields} }} }}",
            Variables = new { input }
        };

        var response = await _client.SendMutationAsync<OrderResponse>(request, cancellationToken);
        return response.Data?.AddOrderItem;
    }

    private class OrdersResponse
    {
        public List<Order> Orders { get; set; } = new();
        public OrderStatistics OrderStats { get; set; } = new();
    }

    private class OrderResponse
    {
        public Order? Order { get; set; }
        public Order? CreateOrder { get; set; }
        public Order? UpdateOrder { get; set; }
        public Order? AddOrderItem { get; set; }
    }

    private class DeleteOrderResponse
    {
        public bool DeleteOrder { get; set; }
    }

    private async Task EnsureAuthAsync()
    {
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            _client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _client.HttpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
