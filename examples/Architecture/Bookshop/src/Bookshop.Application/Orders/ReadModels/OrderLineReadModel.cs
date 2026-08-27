namespace Bookshop.Application.Orders.ReadModels;

/// <summary>
/// A read-only projection of an <see cref="Bookshop.Domain.Orders.OrderLine"/>. The unit price
/// is flattened into <see cref="UnitPrice"/>/<see cref="Currency"/>, matching
/// <see cref="Bookshop.Application.Books.ReadModels.BookReadModel"/>.
/// </summary>
/// <remarks>
/// Parameterless-constructible with init-only properties (not a positional record): GreenDonut's
/// <c>ExpressionHelpers.Rewrite</c> requires a default constructor to build the <c>query.Include</c>
/// projection expression used by the source-generated DataLoaders.
/// </remarks>
public sealed record OrderLineReadModel
{
    public required Guid Id { get; init; }

    public required Guid BookId { get; init; }

    public required int Quantity { get; init; }

    public required decimal UnitPrice { get; init; }

    public required string Currency { get; init; }
}
