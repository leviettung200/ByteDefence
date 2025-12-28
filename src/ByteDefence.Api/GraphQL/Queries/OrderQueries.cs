using System.Security.Claims;
using ByteDefence.Api.Services;
using ByteDefence.Shared.DTOs;
using ByteDefence.Shared.Models;
using HotChocolate;
using HotChocolate.Types;

namespace ByteDefence.Api.GraphQL.Queries;

[QueryType]
public class OrderQueries
{
    [UseFiltering]
    [UseSorting]
    public async Task<IReadOnlyList<Order>> GetOrders([Service] IOrderService service, CancellationToken cancellationToken)
    {
        return await service.GetOrdersAsync(cancellationToken);
    }

    public async Task<Order?> GetOrder(string id, [Service] IOrderService service, CancellationToken cancellationToken)
    {
        return await service.GetOrderAsync(id, cancellationToken);
    }

    public async Task<OrderStatistics> GetOrderStats([Service] IOrderService service, CancellationToken cancellationToken)
    {
        return await service.GetStatisticsAsync(cancellationToken);
    }

    public User Me([Service] IAuthService authService, [GlobalState("currentUser")] ClaimsPrincipal currentUser)
    {
        var user = authService.GetUserFromPrincipal(currentUser);
        if (user is null)
        {
            throw new GraphQLException(ErrorBuilder.New().SetMessage("Unauthorized").SetCode("UNAUTHENTICATED").Build());
        }

        return user;
    }
}
