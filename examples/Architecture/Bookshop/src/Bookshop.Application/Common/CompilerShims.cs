namespace System.Runtime.CompilerServices;

// The C# preview "union" language feature (LangVersion=preview) compiles union
// declarations (see Common/Result.cs) against these two well-known compiler support
// types. The net10.0 reference pack does not ship them yet because the feature is
// still preview-only, so this project declares public in-source shims once, the same
// way IsExternalInit used to be hand-declared before init-only setters shipped in the
// BCL. Every assembly that references Bookshop.Application shares this compiled
// definition instead of redeclaring its own, which would conflict.

/// <summary>
/// Marker interface the compiler makes the generated union wrapper type implement (the case
/// records themselves do not implement it); the wrapper boxes the active case via <see cref="Value"/>.
/// </summary>
public interface IUnion
{
    object? Value { get; }
}

/// <summary>
/// Marks a type declaration as a preview union. The compiler requires this attribute
/// type to exist; it does not read any state from it.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UnionAttribute : Attribute
{
}
