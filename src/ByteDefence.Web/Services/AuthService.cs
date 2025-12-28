using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ByteDefence.Shared.DTOs;
using ByteDefence.Shared.Models;
using Microsoft.JSInterop;

namespace ByteDefence.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "bytedefence_token";
    private const string UserKey = "bytedefence_user";

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/login", new LoginRequest { Username = username, Password = password });
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var authResult = await response.Content.ReadFromJsonAsync<AuthResult>();
        if (authResult is null)
        {
            return false;
        }

        await SaveAsync(authResult);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", UserKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<User>(json);
    }

    public async Task AttachTokenAsync(HttpRequestMessage message)
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task SaveAsync(AuthResult auth)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, auth.Token);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserKey, JsonSerializer.Serialize(auth.User));
    }
}
