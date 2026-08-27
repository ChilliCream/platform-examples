namespace Bookshop.Application.Common;

/// <summary>
/// The use case completed successfully and produced <paramref name="Value"/>.
/// </summary>
public sealed record Success<T>(T Value);

/// <summary>
/// A referenced entity does not exist.
/// </summary>
public sealed record NotFound(string Reason);

/// <summary>
/// The request violates a business rule.
/// </summary>
public sealed record ValidationError(string Reason);

/// <summary>
/// A small discriminated-union-style result for use cases that have expected failure
/// modes (a missing entity, a broken business rule). Handlers return this instead of
/// throwing, so callers can pattern match on the outcome without a try/catch.
/// </summary>
/// <remarks>
/// Declared with the C# preview "union" language feature (<c>LangVersion=preview</c>)
/// instead of a hand-rolled abstract record hierarchy: <see cref="Success{T}"/>,
/// <see cref="NotFound"/> and <see cref="ValidationError"/> are plain sealed records
/// that convert implicitly into <see cref="Result{T}"/>. The feature needs two
/// well-known compiler support types that the net10.0 reference pack does not yet
/// ship (it is preview-only); this project declares in-source shims for them once,
/// in <c>Common/CompilerShims.cs</c>, shared by every assembly that references
/// Bookshop.Application.
/// </remarks>
/// <typeparam name="T">The type of the value produced on success.</typeparam>
public union Result<T>(Success<T>, NotFound, ValidationError);
