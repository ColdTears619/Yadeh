# Yadeh

Yadeh is a virtual memory app designed to turn conversations into lasting knowledge.
Its goal is to extract key points from chats so you can remember important details
and revisit what matters.

The project is in early development. Conversation importing and key point
extraction are planned features and are not yet available.

## Technology Stack

- C# and .NET 8
- ASP.NET Core
- Blazor with Interactive Server rendering
- Razor Components and CSS

## Getting Started

Install the .NET 8 SDK, which includes the ASP.NET Core 8 runtime.

Run the following commands from the repository root:

```sh
dotnet restore Yadeh.sln
dotnet run --project src/Yadeh.Web --launch-profile http
```

Open [localhost:5050](http://localhost:5050) in your browser.

To run with HTTPS, first configure and trust the development HTTPS certificate for your system, then run:

```sh
dotnet run --project src/Yadeh.Web --launch-profile https
```

The HTTPS endpoint is [localhost:7018](https://localhost:7018).

## Build

```sh
dotnet build Yadeh.sln --configuration Release
```

For project structure and implementation notes, see the [developer documentation](doc/README.md).
