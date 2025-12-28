using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;

namespace ByteDefence.Api.Middleware;

public static class FunctionContextExtensions
{
    private const string PrincipalKey = "UserPrincipal";

    public static ClaimsPrincipal? GetPrincipal(this FunctionContext context)
    {
        if (context.Items.TryGetValue(PrincipalKey, out var value) && value is ClaimsPrincipal principal)
        {
            return principal;
        }

        return null;
    }
}
