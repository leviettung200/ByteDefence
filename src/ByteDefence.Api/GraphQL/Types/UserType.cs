using ByteDefence.Shared.Models;
using HotChocolate.Types;

namespace ByteDefence.Api.GraphQL.Types;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Field(u => u.Id).Type<NonNullType<IdType>>();
        descriptor.Field(u => u.Username).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.DisplayName).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.Role).Type<NonNullType<EnumType<UserRole>>>();
    }
}
