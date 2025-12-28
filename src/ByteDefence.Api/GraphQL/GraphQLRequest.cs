using System.Text.Json.Serialization;

namespace ByteDefence.Api.GraphQL;

public class GraphQLRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("operationName")]
    public string? OperationName { get; set; }

    [JsonPropertyName("variables")]
    public Dictionary<string, object?>? Variables { get; set; }
}
