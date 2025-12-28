using System.Security.Claims;
using ByteDefence.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace ByteDefence.Api.Middleware;

public class BearerAuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IAuthService _authService;
    private const string PrincipalKey = "UserPrincipal";

    public BearerAuthenticationMiddleware(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Skip auth for the login function
        if (string.Equals(context.FunctionDefinition.Name, "Login", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var request = await context.GetHttpRequestDataAsync();
        if (request is null)
        {
            await next(context);
            return;
        }

        var principal = ExtractPrincipal(request);
        if (principal is not null)
        {
            context.Items[PrincipalKey] = principal;
        }

        await next(context);
    }

    private ClaimsPrincipal? ExtractPrincipal(HttpRequestData request)
    {
        if (!request.Headers.TryGetValues("Authorization", out var headers))
        {
            return null;
        }

        var bearer = headers.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(bearer))
        {
            return null;
        }

        var token = bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? bearer[7..]
            : bearer;

        return _authService.ValidateToken(token);
    }
}
