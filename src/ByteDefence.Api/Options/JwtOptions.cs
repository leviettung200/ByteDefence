namespace ByteDefence.Api.Options;

public class JwtOptions
{
    public string Secret { get; set; } = "dev-secret";
    public string Issuer { get; set; } = "bytedefence-local";
    public string Audience { get; set; } = "bytedefence-clients";
    public int ExpiryMinutes { get; set; } = 60;
}
