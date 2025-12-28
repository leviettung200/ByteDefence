using System.Security.Claims;
using ByteDefence.Shared.Models;
using ByteDefence.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace ByteDefence.Web.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;

    public CustomAuthStateProvider(AuthService authService)
    {
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user is null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        return new AuthenticationState(new ClaimsPrincipal(CreateIdentity(user)));
    }

    public void NotifyUserChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static ClaimsIdentity CreateIdentity(User user)
    {
        return new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("displayName", user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        }, "jwt");
    }
}
