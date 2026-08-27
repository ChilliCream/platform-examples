namespace Bookshop.Application.Books.ReadModels;

/// <summary>
/// A read-only projection of a <see cref="Bookshop.Domain.Books.Book"/>, independent of the
/// aggregate. Price and currency use the same primitive shape as
/// <see cref="Bookshop.Application.Orders.ReadModels.OrderLineReadModel"/>.
/// Returned by book DataLoaders; never the aggregate itself.
/// </summary>
/// <remarks>
/// Parameterless-constructible with init-only properties (not a positional record): GreenDonut's
/// <c>ExpressionHelpers.Rewrite</c> requires a default constructor to build the <c>query.Include</c>
/// projection expression used by the source-generated DataLoaders.
/// </remarks>
public sealed record BookReadModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Isbn { get; init; }

    public required decimal Price { get; init; }

    public required string Currency { get; init; }

    public required Guid AuthorId { get; init; }

    public required int StockQuantity { get; init; }
}
