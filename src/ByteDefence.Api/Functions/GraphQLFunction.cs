using System.Net;
using System.Net.Mime;
using System.Text.Json;
using ByteDefence.Api.GraphQL;
using ByteDefence.Api.Middleware;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace ByteDefence.Api.Functions;

public class GraphQLFunction
{
    private readonly IRequestExecutorResolver _executorResolver;

    public GraphQLFunction(IRequestExecutorResolver executorResolver)
    {
        _executorResolver = executorResolver;
    }

    [Function("GraphQL")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "graphql")] HttpRequestData req,
        FunctionContext executionContext)
    {
        if (string.Equals(req.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            var preflight = req.CreateResponse(HttpStatusCode.NoContent);
            preflight.Headers.Add("Access-Control-Allow-Origin", "*");
            preflight.Headers.Add("Access-Control-Allow-Methods", "POST,OPTIONS");
            preflight.Headers.Add("Access-Control-Allow-Headers", "authorization,content-type");
            return preflight;
        }

        var principal = executionContext.GetPrincipal();
        if (principal is null)
        {
            return Unauthorized(req);
        }

        var gqlRequest = await ParseRequestAsync(req);
        if (gqlRequest is null || string.IsNullOrWhiteSpace(gqlRequest.Query))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing GraphQL query");
            return bad;
        }

        var executor = await _executorResolver.GetRequestExecutorAsync();
        var request = QueryRequestBuilder.New()
            .SetQuery(gqlRequest.Query)
            .SetOperation(gqlRequest.OperationName)
            .SetVariableValues(gqlRequest.Variables)
            .SetGlobalState("currentUser", principal)
            .Create();

        var result = await executor.ExecuteAsync(request);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", MediaTypeNames.Application.Json);
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        await response.WriteStringAsync(result.ToJson());
        return response;
    }

    private static HttpResponseData Unauthorized(HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.Unauthorized);
        response.Headers.Add("WWW-Authenticate", "Bearer");
        return response;
    }

    private static async Task<GraphQLRequest?> ParseRequestAsync(HttpRequestData req)
    {
        return await JsonSerializer.DeserializeAsync<GraphQLRequest>(req.Body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
