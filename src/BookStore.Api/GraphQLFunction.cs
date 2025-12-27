using HotChocolate.AzureFunctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace BookStore.Api;

public class GraphQLFunction
{
    private readonly IGraphQLRequestExecutor _executor;

    public GraphQLFunction(IGraphQLRequestExecutor executor)
    {
        _executor = executor;
    }

    [Function("GraphQL")]
    public Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "graphql/{**slug}")] 
        HttpRequest request)
    {
        return _executor.ExecuteAsync(request);
    }
}
