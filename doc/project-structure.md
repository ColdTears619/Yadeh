# Project Structure

Yadeh is a .NET 8 solution organized into Domain, Application, Infrastructure,
and Web layers. Tests are separated by the layer they exercise.

## Directory Layout

```text
Yadeh/
├── .config/
│   └── dotnet-tools.json
├── doc/
│   ├── README.md
│   └── project-structure.md
├── src/
│   ├── Yadeh.Domain/
│   │   ├── Conversations/
│   │   │   ├── Enums/
│   │   │   ├── Conversation.cs
│   │   │   ├── ConversationMessage.cs
│   │   │   └── ConversationSnapshot.cs
│   │   └── SharedChats/
│   │       └── ChatGptSharedChatLink.cs
│   ├── Yadeh.Application/
│   │   ├── Common/Contracts/
│   │   ├── Conversations/Contracts/
│   │   └── SharedChats/
│   │       ├── Contracts/
│   │       ├── Enums/
│   │       └── Models/
│   ├── Yadeh.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   ├── Repositories/
│   │   │   ├── YadehDbContext.cs
│   │   │   └── YadehUnitOfWork.cs
│   │   └── SharedChats/
│   │       ├── Parsing/
│   │       └── ChatGptSharedChatProbeClient.cs
│   └── Yadeh.Web/
│       ├── Components/
│       │   ├── Layout/
│       │   └── Pages/
│       ├── wwwroot/
│       ├── Program.cs
│       └── appsettings.json
└── tests/
    ├── Yadeh.Domain.Tests/
    └── Yadeh.Infrastructure.Tests/
```

Generated `bin/` and `obj/` directories and individual source files that do not
affect the architectural overview are omitted.

## Dependency Direction

| Project | References | Responsibility |
| --- | --- | --- |
| `Yadeh.Domain` | None | Business entities, value objects, invariants, and conversation versioning. |
| `Yadeh.Application` | Domain | Use-case models and contracts for persistence and external sources. |
| `Yadeh.Infrastructure` | Application, Domain | HTTP integrations, parsing, EF Core, SQLite, repositories, and migrations. |
| `Yadeh.Web` | Application, Infrastructure | Blazor UI, configuration, middleware, and dependency composition. |

The Domain project has no dependency on EF Core, ASP.NET Core, or provider APIs.
Application code depends on abstractions while Infrastructure supplies their
implementations.

## Conversation Model

`Conversation` is the aggregate root. It has an editable title and owns one or
more `ConversationSnapshot` entities. Each snapshot records:

- the original title reported by the source;
- the source share URL;
- the UTC import time;
- an ordered collection of `ConversationMessage` entities.

A share URL is intentionally not unique. Importing an updated link can add a new
snapshot to an existing conversation, while the user may also choose to create a
separate conversation from the same link. Message sequence is unique within each
snapshot.

## Shared Chat Retrieval

The current ChatGPT integration has three responsibilities:

1. `ChatGptSharedChatLink` validates ChatGPT share URLs.
2. `ChatGptSharedChatProbeClient` downloads the public page with size and timeout
   limits.
3. `ChatGptSharedChatPageParser` decodes the page and returns the provider-neutral
   `SharedChatSnapshot` application model.

The next import step adds `ISharedChatSourceAdapter`. A ChatGPT adapter will
translate a general source URL into the existing ChatGPT client contract. A small
Resolver will locate the adapter whose `CanHandle` method accepts the URL. Once an
adapter returns `SharedChatSnapshot`, preview, merge selection, and persistence
follow one shared application workflow.

## Persistence

`YadehDbContext` maps the aggregate to three SQLite tables:

```text
Conversations
    └── ConversationSnapshots
            └── ConversationMessages
```

Entity configurations live in separate files under
`Persistence/Configurations`. Both relationships use cascade deletion. The
database also enforces non-negative message sequence values and a unique
`ConversationSnapshotId` plus `Sequence` index.

`IConversationRepository` provides aggregate retrieval and mutation operations.
`IUnitOfWork` defines the save boundary, and `YadehUnitOfWork` delegates it to EF
Core's `SaveChangesAsync`.

The initial migration lives in `Persistence/Migrations`. The repository-local
`dotnet-ef` version is recorded in `.config/dotnet-tools.json`.

## Web Entry Point and Rendering

`src/Yadeh.Web/Program.cs` reads the `YadehDatabase` connection string, registers
Infrastructure services, configures middleware, and maps Razor components with
Interactive Server rendering.

`Components/Pages/Home.razor` currently accepts a ChatGPT share link, validates
it, downloads it, and displays the parsed title and messages. It does not yet
persist that preview. The upcoming conversation import application service will
replace the page's direct dependency on the ChatGPT probe contract.

Interactive Server handles component events on the server through a live browser
connection. The application process must remain running for interactive features
to work.

## Configuration

| File | Purpose |
| --- | --- |
| `appsettings.json` | Base logging, host, and SQLite connection-string configuration. |
| `appsettings.Development.json` | Development-environment overrides. |
| `Properties/launchSettings.json` | Local HTTP and HTTPS launch profiles. |

The default SQLite connection string is `Data Source=yadeh.db`. SQLite data files
are local runtime artifacts and are excluded by `.gitignore`.

## Tests

`Yadeh.Domain.Tests` covers URL validation and conversation aggregate invariants.
`Yadeh.Infrastructure.Tests` covers ChatGPT page parsing and repository behavior
against an in-memory SQLite database. Persistence tests verify complete aggregate
round trips, ordering, source-URL lookup, rename persistence, and cascade deletion.

Run all tests with:

```sh
dotnet test Yadeh.sln --configuration Release
```

## Current Boundaries

Implemented:

- ChatGPT share-link validation, download, and parsing;
- conversation, snapshot, and message domain models;
- SQLite persistence, migrations, repository, and Unit of Work;
- automated Domain and Infrastructure tests.

In progress:

- provider-neutral Adapter and Resolver;
- preview and import application workflow;
- Blazor flows for creating or updating saved conversations.

Planned:

- conversation library pages and deletion controls;
- knowledge extraction and search;
- support for additional shared-chat providers;
- authentication if Yadeh evolves beyond local single-user use.
