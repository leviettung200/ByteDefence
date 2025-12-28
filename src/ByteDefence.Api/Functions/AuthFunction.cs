using System.Net;
using ByteDefence.Api.Services;
using ByteDefence.Shared.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ByteDefence.Api.Functions;

public class AuthFunction
{
    private readonly IAuthService _authService;

    public AuthFunction(IAuthService authService)
    {
        _authService = authService;
    }

    [Function("Login")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/login")] HttpRequestData req)
    {
        // Handle CORS preflight
        if (string.Equals(req.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            var preflight = req.CreateResponse(HttpStatusCode.NoContent);
            AddCorsHeaders(preflight);
            return preflight;
        }

        var request = await req.ReadFromJsonAsync<LoginRequest>();
        if (request is null)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid request payload");
            AddCorsHeaders(bad);
            return bad;
        }

        var result = await _authService.LoginAsync(request);
        if (result is null)
        {
            var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorized.WriteStringAsync("Invalid credentials");
            AddCorsHeaders(unauthorized);
            return unauthorized;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        AddCorsHeaders(response);
        return response;
    }

    private static void AddCorsHeaders(HttpResponseData response)
    {
        // Allow local dev origin and common headers/methods used by the app
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
    }
}
