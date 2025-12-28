using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using ByteDefence.Api.Options;
using ByteDefence.Shared.DTOs;
using ByteDefence.Shared.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ByteDefence.Api.Services;

public class AuthService : IAuthService
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly JwtOptions _jwtOptions;
    private readonly SigningCredentials _credentials;
    private readonly TokenValidationParameters _validationParameters;
    private readonly IDictionary<string, (string Password, User User)> _users;

    public AuthService(IOptions<JwtOptions> options)
    {
        _jwtOptions = options.Value;
        // Ensure the signing key is at least 256 bits for HS256. If the configured secret
        // is shorter, derive a 256-bit key using SHA-256 so token creation won't fail.
        var secretBytes = Encoding.UTF8.GetBytes(_jwtOptions.Secret ?? string.Empty);
        if (secretBytes.Length < 32)
        {
            secretBytes = SHA256.HashData(secretBytes);
        }
        var key = new SymmetricSecurityKey(secretBytes);
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        _validationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAudience = _jwtOptions.Audience,
            ValidIssuer = _jwtOptions.Issuer,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromSeconds(5)
        };

        _users = new Dictionary<string, (string Password, User User)>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = ("admin123", new User
            {
                Id = "user-admin",
                Username = "admin",
                DisplayName = "Administrator",
                Role = UserRole.Admin
            }),
            ["user"] = ("user123", new User
            {
                Id = "user-analyst",
                Username = "user",
                DisplayName = "Analyst",
                Role = UserRole.User
            })
        };
    }

    public async Task<AuthResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (!_users.TryGetValue(request.Username, out var record))
        {
            return null;
        }

        if (!string.Equals(record.Password, request.Password, StringComparison.Ordinal))
        {
            return null;
        }

        var token = await Task.FromResult(CreateToken(record.User));
        return new AuthResult
        {
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            User = record.User
        };
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var principal = _tokenHandler.ValidateToken(token, _validationParameters, out var _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public User? GetUserFromPrincipal(ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return null;
        }

        var username = principal.Identity?.Name ?? principal.FindFirstValue(JwtRegisteredClaimNames.UniqueName);
        if (string.IsNullOrWhiteSpace(username))
        {
            username = principal.FindFirstValue("preferred_username");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        if (_users.TryGetValue(username, out var record))
        {
            return record.User;
        }

        var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return _users.Values.Select(u => u.User).FirstOrDefault(u => string.Equals(u.Id, userId, StringComparison.OrdinalIgnoreCase));
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new("name", user.DisplayName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = _credentials
        };

        var token = _tokenHandler.CreateToken(descriptor);
        return _tokenHandler.WriteToken(token);
    }
}
