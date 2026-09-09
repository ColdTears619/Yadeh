# Yadeh

Yadeh is a web application built with **ASP.NET Core and Blazor**, targeting **.NET 8**.

The project is in its initial setup phase. It currently includes the application shell, a default home page, and an error page. Product features have not yet been implemented.

## Technology Stack

- C# and .NET 8
- ASP.NET Core
- Blazor with Interactive Server rendering
- Razor Components and CSS

## Getting Started

Install the .NET 8 SDK, which includes the ASP.NET Core 8 runtime. The repository's
`global.json` selects an installed .NET 8 SDK for consistent local and CI builds.

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

## Automatic Pull Request Merging

The `Build and merge pull requests` workflow runs when a pull request targeting
`main` is opened, reopened, updated, or marked ready for review:

1. Check out GitHub's proposed merge of the pull request with `main`.
2. Install .NET 8, restore dependencies, and build the solution in Release mode.
3. Only after the build succeeds, merge eligible pull requests using a merge
   commit. A failed or cancelled build prevents the merge job from running.

Automatic merging applies only to non-draft pull requests authored by the
repository owner from branches in this repository. Drafts and fork pull requests
are built but are not automatically merged. The build job has read-only access;
the separate merge job uses the built-in `GITHUB_TOKEN` with write permissions
and does not run pull request code. No additional secret is required.

The merge command checks that the pull request's head still matches the revision
that triggered the successful build. It does not queue a deferred auto-merge;
new commits must pass a new build. Required checks and reviews still apply. If
they block merging, satisfy them and re-run the workflow. Resolve conflicts and
push the updated branch to trigger a new build.

The build checks compilation, not application behavior. There is currently no
automated test project; a test step should be added when tests are introduced.

One-time setup:

1. Include `.github/workflows/auto-merge.yml` and `global.json` in the pull request
   and merge them into `main` to establish the workflow for future changes.
2. Keep **Settings → General → Pull Requests → Allow merge commits** enabled.
   **Allow auto-merge** is not required by this workflow.
3. Ensure GitHub Actions is enabled and repository policies permit this
   workflow's `contents: write` and `pull-requests: write` permissions.

Pushing a branch does not create a pull request. Open a pull request targeting
`main` after pushing; subsequent pushes to that open pull request trigger the
workflow again. To enforce this build for manual merges too, configure `Build`
as a required status check in the branch rules for `main`.

## Current Status

- Blazor Interactive Server is configured.
- Database integration and authentication have not yet been added.
- A test project has not yet been created.

This README will be updated as Yadeh's features are developed.
