using System.Security.Claims;
using ByteDefence.Api.GraphQL.Directives;
using ByteDefence.Api.Services;
using ByteDefence.Shared.DTOs;
using ByteDefence.Shared.Models;
using HotChocolate;
using HotChocolate.Types;

namespace ByteDefence.Api.GraphQL.Mutations;

[MutationType]
public class OrderMutations
{
    [RequireRole(UserRole.User)]
    public async Task<Order> CreateOrder(CreateOrderInput input, [Service] IOrderService orderService, [Service] IAuthService authService, [GlobalState("currentUser")] ClaimsPrincipal currentUser, CancellationToken cancellationToken)
    {
        var user = authService.GetUserFromPrincipal(currentUser) ?? throw Unauthorized();
        return await orderService.CreateOrderAsync(input, user, cancellationToken);
    }

    [RequireRole(UserRole.User)]
    public async Task<Order> UpdateOrder(UpdateOrderInput input, [Service] IOrderService orderService, [Service] IAuthService authService, [GlobalState("currentUser")] ClaimsPrincipal currentUser, CancellationToken cancellationToken)
    {
        var user = authService.GetUserFromPrincipal(currentUser) ?? throw Unauthorized();
        return await orderService.UpdateOrderAsync(input, user, cancellationToken);
    }

    [RequireRole(UserRole.Admin)]
    public async Task<bool> DeleteOrder(string id, [Service] IOrderService orderService, CancellationToken cancellationToken)
    {
        return await orderService.DeleteOrderAsync(id, cancellationToken);
    }

    [RequireRole(UserRole.User)]
    public async Task<Order> AddOrderItem(AddOrderItemInput input, [Service] IOrderService orderService, [Service] IAuthService authService, [GlobalState("currentUser")] ClaimsPrincipal currentUser, CancellationToken cancellationToken)
    {
        var user = authService.GetUserFromPrincipal(currentUser) ?? throw Unauthorized();
        return await orderService.AddOrderItemAsync(input, user, cancellationToken);
    }

    private static GraphQLException Unauthorized() => new(ErrorBuilder.New().SetMessage("Unauthorized").SetCode("UNAUTHENTICATED").Build());
}
