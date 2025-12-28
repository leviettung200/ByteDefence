using System.Reflection;
using System.Security.Claims;
using ByteDefence.Shared.Models;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace ByteDefence.Api.GraphQL.Directives;

/// <summary>
/// Minimal role guard middleware that reads the current ClaimsPrincipal from the GraphQL context data.
/// </summary>
public class RequireRoleAttribute : ObjectFieldDescriptorAttribute
{
    private readonly UserRole _role;

    public RequireRoleAttribute(UserRole role)
    {
        _role = role;
    }

    protected override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
    {
        descriptor.Use(next => async ctx =>
        {
            if (!ctx.ContextData.TryGetValue("currentUser", out var value) || value is not ClaimsPrincipal principal)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Unauthorized")
                    .SetCode("UNAUTHENTICATED")
                    .Build());
            }

            if (!principal.IsInRole(_role.ToString()))
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Forbidden")
                    .SetCode("FORBIDDEN")
                    .Build());
            }

            await next(ctx);
        });
    }
}
