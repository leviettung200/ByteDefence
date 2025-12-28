using ByteDefence.Shared.Models;
using HotChocolate.Types;

namespace ByteDefence.Api.GraphQL.Types;

public class OrderType : ObjectType<Order>
{
    protected override void Configure(IObjectTypeDescriptor<Order> descriptor)
    {
        descriptor.Description("Represents a purchase order with nested items.");

        descriptor.Field(o => o.Id).Type<NonNullType<IdType>>();
        descriptor.Field(o => o.Title).Type<NonNullType<StringType>>();
        descriptor.Field(o => o.Status).Type<NonNullType<EnumType<OrderStatus>>>();
        descriptor.Field(o => o.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(o => o.UpdatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(o => o.Items).Type<ListType<NonNullType<OrderItemType>>>();
        descriptor.Field(o => o.CreatedBy).Type<UserType>();
        descriptor.Field("total").Type<NonNullType<DecimalType>>()
            .Resolve(ctx => ctx.Parent<Order>().Total)
            .Description("Computed total of all order items");
    }
}
