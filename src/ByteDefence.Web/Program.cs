using ByteDefence.Web;
using ByteDefence.Web.Auth;
using ByteDefence.Web.Services;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["Api:Url"] ?? "http://localhost:7071/api/";
var graphQlEndpoint = builder.Configuration["Api:GraphQl"] ?? $"{apiBase.TrimEnd('/')}/graphql";

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBase) });

builder.Services.AddScoped(sp =>
{
	var httpClient = new HttpClient
	{
		BaseAddress = new Uri(graphQlEndpoint)
	};

	return new GraphQLHttpClient(new GraphQLHttpClientOptions
	{
		EndPoint = new Uri(graphQlEndpoint)
	}, new SystemTextJsonSerializer(), httpClient);
});

builder.Services.AddScoped<OrderApiClient>();
builder.Services.AddScoped<SignalRService>();

await builder.Build().RunAsync();
