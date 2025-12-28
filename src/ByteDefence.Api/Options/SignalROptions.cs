namespace ByteDefence.Api.Options;

public class SignalROptions
{
    public string Mode { get; set; } = "Local"; // Local | Azure | None
    public string HubUrl { get; set; } = "http://localhost:5000";
    public string? AccessKey { get; set; }
}
