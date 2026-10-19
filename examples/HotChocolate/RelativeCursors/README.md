# Offset Pagination is Dead! Meet Relative Cursors

This example shows the paging capabilities of Hot Chocolate 15.1, including relative cursors, which make offset pagination obsolete. The API is a small product catalog backed by PostgreSQL, with products and brands exposed as paged GraphQL connections.

- Video: [https://www.youtube.com/watch?v=8TQ2oDUQ1ng](https://www.youtube.com/watch?v=8TQ2oDUQ1ng)
- Episode page: [https://chillicream.com/videos/2025-03-17-relative-cursors](https://chillicream.com/videos/2025-03-17-relative-cursors)

Hot Chocolate version: 15.1.0-p.18

## Prerequisites

- .NET SDK 9.0 or later (the project targets `net9.0`)
- Docker, to run PostgreSQL with the included `compose.yml`

## Run

From this folder, start PostgreSQL:

```bash
docker compose up -d
```

The compose file starts PostgreSQL (`ankane/pgvector`) on port 5432 with user `admin` and password `root`. It also starts pgAdmin on port 5050. The connection string is set in `src/Catalog.API/appsettings.json`. The database `CatalogDB` is created and migrated on startup.

Then start the API:

```bash
dotnet run --project src/Catalog.API
```

The GraphQL endpoint is `http://localhost:5224/graphql`, the URL from the `http` profile in `src/Catalog.API/Properties/launchSettings.json`.

The solution file `src/eShop.slnx` contains the `Catalog.API` project. Build it with:

```bash
dotnet build src/eShop.slnx
```

## Notes

The local development credentials (`admin` / `root`) in `compose.yml` and `appsettings.json` are for local use only.
