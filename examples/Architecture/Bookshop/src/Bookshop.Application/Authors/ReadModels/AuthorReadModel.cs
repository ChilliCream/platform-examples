namespace Bookshop.Application.Authors.ReadModels;

/// <summary>
/// A read-only projection of an <see cref="Bookshop.Domain.Authors.Author"/>, independent of
/// the aggregate. Returned by author DataLoaders; never the aggregate itself.
/// </summary>
/// <remarks>
/// Parameterless-constructible with init-only properties (not a positional record): GreenDonut's
/// <c>ExpressionHelpers.Rewrite</c> requires a default constructor to build the <c>query.Include</c>
/// projection expression used by the source-generated DataLoaders.
/// </remarks>
public sealed record AuthorReadModel
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string? Bio { get; init; }
}
