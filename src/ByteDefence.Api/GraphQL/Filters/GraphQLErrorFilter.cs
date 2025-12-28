using HotChocolate;
using HotChocolate.Execution;

namespace ByteDefence.Api.GraphQL.Filters;

public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is UnauthorizedAccessException)
        {
            return error.WithMessage("Unauthorized").WithCode("UNAUTHENTICATED");
        }

        if (error.Exception is InvalidOperationException)
        {
            return error.WithCode("BAD_REQUEST");
        }

        if (error.Exception is { } ex)
        {
            return error.WithMessage(ex.Message).WithCode("SERVER_ERROR");
        }

        return error;
    }
}
