using ByteDefence.Shared.Models;

namespace ByteDefence.Shared.DTOs;

public class AuthResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public User User { get; set; } = new();
}
