# platform-examples

Standalone, runnable example applications for the ChilliCream platform products
(currently [Mocha](https://github.com/ChilliCream/graphql-platform/tree/main/src/Mocha) and
[Hot Chocolate](https://github.com/ChilliCream/graphql-platform/tree/main/src/HotChocolate)).
Examples live here instead of in the main
[graphql-platform](https://github.com/ChilliCream/graphql-platform) repository so
they can be built, run, and versioned independently of the product source and
its release cadence.

## Layout

```
examples/<Product>/<Name>/
```

Each product gets one top-level folder under `examples/`, and each example gets
its own folder inside it. For example:

```
examples/Mocha/Demo/
examples/Mocha/PostgresTransport/
examples/Mocha/AotExample/
examples/Mocha/ExceptionPolicies/
examples/HotChocolate/DataLoaderExplained/
examples/HotChocolate/GreenDonutData/
examples/HotChocolate/LayeredDataLoader/
examples/HotChocolate/RelativeCursors/
examples/HotChocolate/ConnectionAggregations/
examples/HotChocolate/EfCoreProjections/
examples/HotChocolate/StatefulDataLoader/
examples/Architecture/Bookshop/
```

`examples/HotChocolate/` contains the source code of the
[ChilliCream TV](https://chillicream.com/videos) episodes. Each example pins the Hot Chocolate version
used in its episode through its own `Directory.Packages.props`, so the code matches the video.

`examples/Architecture/` contains cross-product architecture examples. `Bookshop` demonstrates
clean architecture and DDD with HotChocolate, GreenDonut, Mocha.Mediator, and EF Core.

## Solution files

Each product has its own solution file at `examples/<Product>/<Product>.Examples.slnx`
(for example `examples/Mocha/Mocha.Examples.slnx`), rather than a single root
solution. Products are independent from a build and dependency standpoint, and
a per-product solution keeps IDE load times and build scope proportional to the
product you are actually working on as the repository grows to cover more
products.

## Repo conventions

- **Central package management**: package versions are pinned in the root
  `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`). Add new
  package versions there, not in individual project files.
- **Shared build settings**: `Directory.Build.props` sets `LangVersion`,
  `Nullable`, and `ImplicitUsings` consistently with the
  [graphql-platform](https://github.com/ChilliCream/graphql-platform)
  conventions.
- **SDK version**: `global.json` pins the .NET SDK to the same version used by
  graphql-platform, so examples build against the same toolchain as the
  products they demonstrate.
