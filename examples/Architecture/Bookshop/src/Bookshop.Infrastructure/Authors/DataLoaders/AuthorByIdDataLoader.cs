using Bookshop.Application.Authors.DataLoaders;

namespace Bookshop.Infrastructure.Authors.DataLoaders;

/// <summary>
/// Binds the source-generated <c>AuthorByIdDataLoader</c> (from
/// <see cref="AuthorDataLoaders.GetAuthorByIdAsync"/>) to the Application-defined
/// <see cref="IAuthorByIdDataLoader"/> interface. This assembly generates DataLoader classes
/// without their own interfaces (<c>DataLoaderDefaults(GenerateInterfaces = false)</c> in
/// Properties/AssemblyInfo.cs), so the generated class is declared <c>partial</c> precisely to
/// let this second declaration attach the interface the generator does not emit itself.
/// Accessibility here must match the generator's own default (public, since
/// <c>DataLoaderDefaults.IsPublic</c> is not overridden).
/// </summary>
public sealed partial class AuthorByIdDataLoader : IAuthorByIdDataLoader;
