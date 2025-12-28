using ByteDefence.Shared.DTOs;
using ByteDefence.Shared.Models;
using System.Security.Claims;

namespace ByteDefence.Api.Services;

public interface IAuthService
{
    Task<AuthResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    ClaimsPrincipal? ValidateToken(string token);
    User? GetUserFromPrincipal(ClaimsPrincipal? principal);
}
