using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BookStore.Api.Auth;

public class JwtAuthenticationService
{
    // For demo purposes, using a static secret key. In production, use proper key management.
    private const string SecretKey = "BookStoreApiSecretKeyForDemoPurposes2024!MustBeAtLeast32Chars";
    private const string StaticDemoToken = "demo-bearer-token-2024";
    
    public static readonly string Issuer = "BookStoreApi";
    public static readonly string Audience = "BookStoreClient";

    private readonly SymmetricSecurityKey _signingKey;

    public JwtAuthenticationService()
    {
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
    }

    /// <summary>
    /// Generates a JWT token for testing purposes.
    /// </summary>
    public string GenerateToken(string userId, string userName, string role = "User")
    {
        var handler = new JsonWebTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        };

        return handler.CreateToken(descriptor);
    }

    /// <summary>
    /// Validates a token and returns the claims principal if valid.
    /// Supports both JWT tokens and the static demo token.
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        // Support static demo token for simple testing
        if (token == StaticDemoToken)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "demo-user"),
                new Claim(ClaimTypes.Name, "Demo User"),
                new Claim(ClaimTypes.Role, "User")
            }, "DemoToken"));
        }

        try
        {
            var handler = new JsonWebTokenHandler();
            var result = handler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = _signingKey
            }).GetAwaiter().GetResult();

            if (result.IsValid)
            {
                return new ClaimsPrincipal(result.ClaimsIdentity);
            }
        }
        catch
        {
            // Token validation failed
        }

        return null;
    }
}
