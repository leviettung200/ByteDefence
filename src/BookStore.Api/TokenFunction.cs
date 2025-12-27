using BookStore.Api.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;

namespace BookStore.Api;

public class TokenFunction
{
    private readonly JwtAuthenticationService _authService;

    public TokenFunction(JwtAuthenticationService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Generates a JWT token for testing purposes.
    /// POST /api/token with body: { "userId": "user-1", "userName": "Test User", "role": "User" }
    /// </summary>
    [Function("GenerateToken")]
    public async Task<IActionResult> GenerateToken(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "token")] 
        HttpRequest request)
    {
        try
        {
            var body = await JsonSerializer.DeserializeAsync<TokenRequest>(request.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (body == null || string.IsNullOrEmpty(body.UserId) || string.IsNullOrEmpty(body.UserName))
            {
                return new BadRequestObjectResult(new { error = "userId and userName are required" });
            }

            var token = _authService.GenerateToken(body.UserId, body.UserName, body.Role ?? "User");

            return new OkObjectResult(new
            {
                token = token,
                expiresIn = 3600,
                tokenType = "Bearer"
            });
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult(new { error = "Invalid JSON body" });
        }
    }

    /// <summary>
    /// Returns auth information for testing, including the static demo token.
    /// </summary>
    [Function("AuthInfo")]
    public IActionResult GetAuthInfo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth-info")] 
        HttpRequest request)
    {
        return new OkObjectResult(new
        {
            message = "BookStore API Authentication Information",
            staticDemoToken = "demo-bearer-token-2024",
            usage = "Add 'Authorization: Bearer <token>' header to your requests",
            generateJwtEndpoint = "POST /api/token with { \"userId\": \"user-1\", \"userName\": \"Test User\" }",
            protectedOperations = new[] { "createBook", "updateBook", "deleteBook", "createAuthor", "createReview" }
        });
    }

    private class TokenRequest
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Role { get; set; }
    }
}
