using ByteDefence.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
	?? new[] { "http://localhost:5001", "http://localhost:7071", "http://localhost:5000" };

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
		policy.WithOrigins(allowedOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials());
});

var app = builder.Build();

app.UseRouting();
app.UseCors();

app.MapGet("/health", () => Results.Ok("ok"));

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapPost("/api/broadcast", async (BroadcastEnvelope envelope, IHubContext<NotificationHub> hubContext) =>
{
	await hubContext.Clients.Group(envelope.Group).SendAsync(envelope.Method, envelope.Data);
	return Results.Ok(new { status = "sent" });
});

app.Run();

public record BroadcastEnvelope(string Method, string Group, object Data);
