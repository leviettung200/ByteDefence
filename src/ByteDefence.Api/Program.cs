using ByteDefence.Api.Data;
using ByteDefence.Api.GraphQL.Filters;
using ByteDefence.Api.GraphQL.Mutations;
using ByteDefence.Api.GraphQL.Queries;
using ByteDefence.Api.GraphQL.Types;
using ByteDefence.Api.Middleware;
using ByteDefence.Api.Options;
using ByteDefence.Api.Services;
using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Functions.Worker.Middleware;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SignalROptions>(builder.Configuration.GetSection("SignalR"));

var useCosmos = builder.Configuration.GetValue<bool>("UseCosmosDb");
if (useCosmos)
{
    var cosmosConnection = builder.Configuration.GetConnectionString("Cosmos") ?? builder.Configuration.GetValue<string>("Cosmos:ConnectionString") ?? string.Empty;
    var cosmosDatabase = builder.Configuration.GetValue<string>("Cosmos:Database") ?? "ByteDefence";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseCosmos(cosmosConnection, cosmosDatabase));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("ByteDefence"));
}

builder.Services
    .AddScoped<IOrderService, OrderService>()
    .AddSingleton<IAuthService, AuthService>()
    .AddHttpClient<LocalNotificationService>()
    .Services
    .AddSingleton<NoOpNotificationService>()
    .AddScoped<INotificationService>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<SignalROptions>>().Value;
        return options.Mode.Equals("Local", StringComparison.OrdinalIgnoreCase)
            ? sp.GetRequiredService<LocalNotificationService>()
            : sp.GetRequiredService<NoOpNotificationService>();
    })
    .AddGraphQLServer()
        .AddQueryType<OrderQueries>()
        .AddMutationType<OrderMutations>()
        .AddType<OrderType>()
        .AddType<OrderItemType>()
        .AddType<UserType>()
        .AddFiltering()
        .AddSorting()
        .AddErrorFilter<GraphQLErrorFilter>()
        .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
        .Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<IFunctionsWorkerMiddleware, BearerAuthenticationMiddleware>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
