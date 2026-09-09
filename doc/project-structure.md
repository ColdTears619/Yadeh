# Project Structure

Yadeh currently consists of a single ASP.NET Core web project using Blazor
Interactive Server. Application code lives in `src/Yadeh.Web`.

## Directory Layout

```text
Yadeh/
├── README.md
├── LICENSE
├── Yadeh.sln
├── global.json
├── doc/
│   ├── README.md
│   └── project-structure.md
└── src/
    └── Yadeh.Web/
        ├── Yadeh.Web.csproj
        ├── Program.cs
        ├── appsettings.json
        ├── appsettings.Development.json
        ├── Properties/
        │   └── launchSettings.json
        ├── Components/
        │   ├── App.razor
        │   ├── Routes.razor
        │   ├── _Imports.razor
        │   ├── Layout/
        │   │   ├── MainLayout.razor
        │   │   └── MainLayout.razor.css
        │   └── Pages/
        │       ├── Home.razor
        │       └── Error.razor
        └── wwwroot/
            └── app.css
```

The tree focuses on application code and documentation. Generated `bin/` and
`obj/` directories are omitted.

## Entry Points and Rendering

| File | Responsibility |
| --- | --- |
| `Yadeh.sln` | Groups the projects built as part of the solution. |
| `global.json` | Selects an installed .NET 8 SDK, allowing newer .NET 8 feature bands. |
| `src/Yadeh.Web/Yadeh.Web.csproj` | Defines the web project, targets `net8.0`, and enables nullable reference types and implicit imports. |
| `src/Yadeh.Web/Program.cs` | Registers Razor components and Interactive Server support, configures middleware, and maps the root component. |
| `src/Yadeh.Web/Components/App.razor` | Defines the HTML document, loads styles and the Blazor script, and applies Interactive Server rendering to the routes and document head. |
| `src/Yadeh.Web/Components/Routes.razor` | Resolves page routes, applies the default layout, and manages focus after navigation. |
| `src/Yadeh.Web/Components/_Imports.razor` | Shares Razor imports across components. |

Interactive Server handles component events on the server through a connection
with the browser. The application must remain running for interactive features
to work.

## Pages, Layout, and Styles

- `Components/Pages/` contains routable Razor components. `Home.razor` serves `/`;
  `Error.razor` provides the error page.
- `Components/Layout/MainLayout.razor` wraps page content and includes the Blazor
  error notification UI. Its accompanying `.razor.css` file provides scoped styles.
- `wwwroot/` contains publicly served static files. `app.css` supplies shared styles.

New routable pages belong in `Components/Pages/`. Shared page layout changes
belong in `Components/Layout/`, while styles specific to a component can live in
an accompanying `.razor.css` file.

## Configuration

| File | Purpose |
| --- | --- |
| `appsettings.json` | Base application configuration, including logging and allowed hosts. |
| `appsettings.Development.json` | Configuration overrides for the Development environment. |
| `Properties/launchSettings.json` | Local launch profiles, development URLs, and environment selection. |

The HTTP launch profile uses port `5050`. The HTTPS profile uses port `7018`
for HTTPS and `5050` for HTTP. Local launch profiles are development settings;
they do not configure a deployed server.

`Program.cs` configures HTTPS redirection, static file serving, and antiforgery
middleware. Outside Development, it also enables HSTS and routes unhandled
exceptions to `/Error`.

## Current Implementation Boundaries

The home page accepts a shared ChatGPT conversation URL and performs basic
validation. It does not yet fetch conversations or extract key points.

Database persistence, authentication, conversation importing, and knowledge
extraction have not yet been implemented. There are currently no separate
service, data-access, or automated test projects.
