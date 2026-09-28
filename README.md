# Yadeh

Yadeh is a personal memory application for turning shared AI conversations into
lasting, searchable knowledge. It preserves imported conversations as snapshots
so an updated share link can be added to an existing conversation without losing
earlier versions.

The application currently supports validating, downloading, and parsing public
ChatGPT share links. The conversation domain model and SQLite persistence layer
are implemented. Saving imports through the Blazor UI, search, knowledge
extraction, and authentication are still in development.

## Technology Stack

- C# and .NET 8
- ASP.NET Core and Blazor Interactive Server
- Entity Framework Core 8 with SQLite
- Razor Components and CSS
- xUnit for domain and infrastructure tests

## Architecture

The solution follows a layered architecture:

```text
Yadeh.Web
    ├── Yadeh.Application
    └── Yadeh.Infrastructure
            ├── Yadeh.Application
            └── Yadeh.Domain

Yadeh.Application
    └── Yadeh.Domain
```

- **Domain** contains conversation aggregates, snapshots, messages, and domain
  validation.
- **Application** defines use-case models and persistence or external-service
  contracts.
- **Infrastructure** implements ChatGPT retrieval and parsing, repositories,
  Entity Framework Core mappings, migrations, and SQLite persistence.
- **Web** hosts the Blazor UI and application composition root.

Repository and Unit of Work patterns isolate persistence from application code.
The next source-integration step introduces a shared-chat Adapter boundary and a
Resolver so future providers can produce the same application snapshot model.

## Getting Started

Install the .NET 8 SDK. SQLite is embedded in the application, so no database
server or Docker container is required.

Restore dependencies and the repository-local Entity Framework tool:

```sh
dotnet restore Yadeh.sln
dotnet tool restore
```

Apply the database migrations:

```sh
dotnet tool run dotnet-ef database update \
  --project src/Yadeh.Infrastructure/Yadeh.Infrastructure.csproj \
  --startup-project src/Yadeh.Web/Yadeh.Web.csproj
```

Run the application over HTTP:

```sh
dotnet run --project src/Yadeh.Web --launch-profile http
```

Open [localhost:5050](http://localhost:5050) in your browser.

To use HTTPS, first configure and trust the ASP.NET Core development certificate,
then run:

```sh
dotnet run --project src/Yadeh.Web --launch-profile https
```

The HTTPS endpoint is [localhost:7018](https://localhost:7018).

## Build and Test

```sh
dotnet build Yadeh.sln --configuration Release
dotnet test Yadeh.sln --configuration Release
```

## Database

The default connection string is configured in
`src/Yadeh.Web/appsettings.json`:

```text
Data Source=yadeh.db
```

SQLite database, shared-memory, and write-ahead log files are ignored by Git.
Schema changes are tracked through migrations in
`src/Yadeh.Infrastructure/Persistence/Migrations`.

Create a migration with:

```sh
dotnet tool run dotnet-ef migrations add YourMigrationName \
  --project src/Yadeh.Infrastructure/Yadeh.Infrastructure.csproj \
  --startup-project src/Yadeh.Web/Yadeh.Web.csproj \
  --output-dir Persistence/Migrations
```

For implementation details, see the
[developer documentation](doc/README.md).
