# The Future of Data APIs: GreenDonut in Action!

This example shows how to use the GreenDonut.Data package to add paging, projections, filtering, and sorting to a GraphQL API with a layered architecture.

- Video: https://www.youtube.com/watch?v=FhNK7KMAnXc
- Episode page: https://chillicream.com/videos/2025-02-27-greendonut-in-action

Hot Chocolate version: 15.1.0-p.5 (`HotChocolate.AspNetCore` and related packages in `Directory.Packages.props`).

## Prerequisites

- .NET SDK (the target framework is net9.0, the repository root `global.json` pins the SDK)
- Docker, to run PostgreSQL with `compose.yml`

## Run

Start PostgreSQL (with pgvector) and pgAdmin:

```bash
docker compose up -d
```

The PostgreSQL credentials are `admin` / `root` on `localhost:5432`, as set in `compose.yml` and `src/Catalog.API/appsettings.json`. These are local development values only. The `CatalogDB` database is created and seeded on startup.

Start the Catalog API:

```bash
cd src/Catalog.API
dotnet run
```

The `http` launch profile listens on `http://localhost:5224`, and the GraphQL endpoint is served at `/graphql`.

## Build

From this folder:

```bash
dotnet build src/eShop.slnx
```
