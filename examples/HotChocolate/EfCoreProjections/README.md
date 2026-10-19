# Boost GraphQL Performance with EF Core Projections

This example shows the projections engine in Hot Chocolate: how EF Core queries are projected from GraphQL selections, with a catalog API (brands, product types, products) backed by PostgreSQL.

- Video: [Boost GraphQL Performance with EF Core Projections](https://www.youtube.com/watch?v=dYSqssul4jY)
- Episode page: [chillicream.com/videos/2025-04-01-ef-core-projections](https://chillicream.com/videos/2025-04-01-ef-core-projections)

Hot Chocolate version: `15.1.0-rc.4` (see `Directory.Packages.props`). The example targets `net9.0` and uses EF Core and Npgsql 9.0.2.

## Prerequisites

- .NET SDK matching the root `global.json` of the platform-examples repository
- Docker, for the PostgreSQL database in `compose.yml`

## Run

Start PostgreSQL (and pgAdmin on port 5050):

```bash
docker compose up -d
```

Run the API:

```bash
dotnet run --project src/Catalog.API
```

The GraphQL endpoint is `/graphql` on `http://localhost:5224` (from `Properties/launchSettings.json`). On startup the app applies the EF Core migrations and seeds the catalog from `src/Catalog.API/Migrations/catalog.json`.

The connection string in `appsettings.json` uses the local development credentials from `compose.yml` (`admin` / `root`). These are for local use only.
