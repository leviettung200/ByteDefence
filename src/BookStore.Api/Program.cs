using BookStore.Api.Auth;
using BookStore.Api.Data;
using BookStore.Api.GraphQL.Queries;
using BookStore.Api.GraphQL.Mutations;
using BookStore.Api.GraphQL.Subscriptions;
using BookStore.Api.GraphQL.Types;
using HotChocolate.AspNetCore;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure EF Core with In-Memory database
builder.Services.AddDbContext<BookStoreDbContext>(options =>
    options.UseInMemoryDatabase("BookStoreDb"));

// Configure Authentication Service
builder.Services.AddSingleton<JwtAuthenticationService>();

// Configure Authorization
builder.Services.AddAuthorization();

// Configure GraphQL with HotChocolate
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddTypeExtension<BookTypeExtension>()
    .AddTypeExtension<AuthorTypeExtension>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization()
    .AddInMemorySubscriptions()
    .AddHttpRequestInterceptor<AuthenticationInterceptor>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookStoreDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
