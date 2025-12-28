using ByteDefence.Shared.Models;
using HotChocolate.Types;

namespace ByteDefence.Api.GraphQL.Types;

public class OrderItemType : ObjectType<OrderItem>
{
    protected override void Configure(IObjectTypeDescriptor<OrderItem> descriptor)
    {
        descriptor.Field(i => i.Id).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.Name).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.Quantity).Type<NonNullType<IntType>>();
        descriptor.Field(i => i.Price).Type<NonNullType<DecimalType>>();
        descriptor.Field("lineTotal").Resolve(ctx => ctx.Parent<OrderItem>().Price * ctx.Parent<OrderItem>().Quantity);
    }
}
