# Yadeh

Yadeh is a web application built with **ASP.NET Core and Blazor**, targeting **.NET 8**.

The project is in its initial setup phase. It currently includes the application shell, a default home page, and an error page. Product features have not yet been implemented.

## Technology Stack

- C# and .NET 8
- ASP.NET Core
- Blazor with Interactive Server rendering
- Razor Components and CSS

## Getting Started

A compatible .NET SDK and the ASP.NET Core 8 runtime are required. Installing the .NET 8 SDK provides both prerequisites.

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

## Project Structure

```text
Yadeh.sln
src/
└── Yadeh.Web/
    ├── Components/
    │   ├── Layout/       # Shared page layouts
    │   ├── Pages/        # Application pages
    │   ├── App.razor     # Root application document
    │   └── Routes.razor  # Routing
    ├── Properties/
    │   └── launchSettings.json
    ├── wwwroot/         # Static assets and styles
    ├── appsettings.json # Application settings
    └── Program.cs       # Service registration and application setup
```

## Current Status

- Blazor Interactive Server is configured.
- Database integration and authentication have not yet been added.
- A test project has not yet been created.

This README will be updated as Yadeh's features are developed.
