using Bookshop.Domain.Orders;

namespace Bookshop.Application.Orders.ReadModels;

/// <summary>
/// A read-only projection of an <see cref="Order"/> aggregate, independent of the aggregate.
/// The total is flattened into <see cref="TotalAmount"/>/<see cref="TotalCurrency"/>, matching
/// <see cref="Bookshop.Application.Books.ReadModels.BookReadModel"/>. Returned by order
/// DataLoaders; never the aggregate itself.
/// </summary>
/// <remarks>
/// Parameterless-constructible with init-only properties (not a positional record): GreenDonut's
/// <c>ExpressionHelpers.Rewrite</c> requires a default constructor to build the <c>query.Include</c>
/// projection expression used by the source-generated DataLoaders.
/// </remarks>
public sealed record OrderReadModel
{
    public required Guid Id { get; init; }

    public required Guid CustomerId { get; init; }

    public required OrderStatus Status { get; init; }

    public required decimal TotalAmount { get; init; }

    public required string TotalCurrency { get; init; }

    public required IReadOnlyList<OrderLineReadModel> Lines { get; init; }
}
