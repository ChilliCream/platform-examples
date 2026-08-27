using GreenDonut;

// This assembly holds the DataLoader IMPLEMENTATIONS (source-generated [DataLoader] fetch
// methods over BookshopContext) for the interfaces Bookshop.Application declares. Application
// depends only on those hand-written interfaces, never on the generated classes or on EF Core,
// so the generator here is configured to emit the DataLoaderBase-derived classes WITHOUT also
// generating their own interfaces (GenerateInterfaces = false) - the interface each generated
// class implements is bound explicitly via a partial class declaration next to the fetch class
// (see e.g. Books/DataLoaders/BookByIdDataLoader.cs), not generated.
[assembly: DataLoaderDefaults(GenerateInterfaces = false)]

// This is a HotChocolate-free data-layer assembly (GreenDonut without GraphQL types), so
// DataLoaderModule (not the GraphQL-aware Module attribute) is the correct registration
// mechanism: it emits an IServiceCollection extension, AddBookshopDataLoaders(), registering
// every generated DataLoader class - called from AddBookshopInfrastructure.
[assembly: DataLoaderModule("BookshopDataLoaders")]
