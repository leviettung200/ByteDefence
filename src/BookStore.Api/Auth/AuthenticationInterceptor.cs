using System.Security.Claims;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;

namespace BookStore.Api.Auth;

public class AuthenticationInterceptor : DefaultHttpRequestInterceptor
{
    private readonly JwtAuthenticationService _authService;

    public AuthenticationInterceptor(JwtAuthenticationService authService)
    {
        _authService = authService;
    }

    public override ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor,
        OperationRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        
        if (!string.IsNullOrEmpty(authHeader))
        {
            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader.Substring(7)
                : authHeader;

            var principal = _authService.ValidateToken(token);
            if (principal != null)
            {
                context.User = principal;
            }
        }

        return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}
