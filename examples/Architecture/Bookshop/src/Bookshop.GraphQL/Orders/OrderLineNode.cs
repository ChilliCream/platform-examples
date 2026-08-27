using Bookshop.Application.Books.ReadModels;
using Bookshop.Application.Orders.ReadModels;
using HotChocolate.Types.Relay;

namespace Bookshop.GraphQL.Orders;

/// <summary>
/// GraphQL shape for <see cref="OrderLineReadModel"/>. <c>BookId</c> is exposed as a relay-id
/// scalar rather than a
/// nested <c>book</c> association field - only <c>Order.lines</c> is called out as an
/// association field to implement per this task's spec, so a nested <c>OrderLine.book</c>
/// association is left out rather than added speculatively; flagged as a gap if the frontend
/// later needs it.
/// </summary>
[ObjectType<OrderLineReadModel>]
internal static partial class OrderLineNode
{
    [ID<OrderLineReadModel>]
    public static Guid GetId([Parent(requires: nameof(OrderLineReadModel.Id))] OrderLineReadModel line)
        => line.Id;

    [ID<BookReadModel>]
    public static Guid GetBookId([Parent(requires: nameof(OrderLineReadModel.BookId))] OrderLineReadModel line)
        => line.BookId;

    static partial void Configure(IObjectTypeDescriptor<OrderLineReadModel> descriptor)
        => descriptor.Name("OrderLine");
}
