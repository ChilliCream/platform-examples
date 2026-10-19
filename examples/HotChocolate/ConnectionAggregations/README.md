# Make Your GraphQL Connections Smarter! Add Aggregations

This example shows how to use the Connections API in Hot Chocolate to add aggregations, such as totals and counts, to GraphQL connections. The aggregates are returned next to the paged data, so clients get insights and items from the same field.

- Video: https://www.youtube.com/watch?v=DtISlxOBmPQ
- Episode page: https://chillicream.com/videos/2025-03-24-connection-aggregations

Hot Chocolate version: `15.1.0-rc.4` (see `Directory.Packages.props`).

## Prerequisites

- .NET SDK, as pinned by the `global.json` of the `platform-examples` repository
- Docker, to run PostgreSQL (`compose.yml` starts PostgreSQL with pgvector and pgAdmin)

## Run

Start PostgreSQL:

```bash
docker compose up -d
```

Run the API from the example folder:

```bash
dotnet run --project src/Catalog.API
```

The database `CatalogDB` is migrated and seeded from `src/Catalog.API/Migrations/catalog.json` on startup. The GraphQL endpoint is `http://localhost:5224/graphql` (from `src/Catalog.API/Properties/launchSettings.json`).

The connection string in `src/Catalog.API/appsettings.json` uses the local development credentials from `compose.yml` (user `admin`, password `root`). pgAdmin is available at `http://localhost:5050` with `admin@admin.com` and password `root`.
