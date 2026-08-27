namespace Bookshop.Application.Customers.ReadModels;

/// <summary>
/// A read-only projection of a <see cref="Bookshop.Domain.Customers.Customer"/>, independent
/// of the aggregate. Returned by customer DataLoaders; never the aggregate itself.
/// </summary>
/// <remarks>
/// Parameterless-constructible with init-only properties (not a positional record): GreenDonut's
/// <c>ExpressionHelpers.Rewrite</c> requires a default constructor to build the <c>query.Include</c>
/// projection expression used by the source-generated DataLoaders.
/// </remarks>
public sealed record CustomerReadModel
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }
}
