# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repository is

This is **Kasei Clean Architecture Solution** (`dotnet new` short name `kasei.arch`, template identity `Kasei.CleanArch`) — a reusable .NET 10 Clean Architecture template, not a single deployed application. It bundles an ASP.NET Core Web API and a companion Avalonia desktop UI, wired together with Repository, Unit of Work, and CQRS (MediatR) patterns over EF Core. The only implemented feature is a CRUD example for a `Country` entity, meant to be copied when adding real entities. `.template.config/template.json` defines the template parameters (connection strings, email settings) that get substituted when someone runs `dotnet new kasei.arch`.

`docs/README.md` is the authoritative, detailed guide for the entity-scaffolding conventions (naming rules, folder locations, DI registration) — read it before adding a new entity end-to-end. The summary below covers the parts most relevant to day-to-day changes.

## Commands

```bash
# Build the whole solution
dotnet build Haskoli.sln

# Run the Web API (from src/Haskoli.Api)
dotnet run --project src/Haskoli.Api/Haskoli.Api.csproj

# Run the Avalonia desktop client
dotnet run --project src/Haskoli.AvaloniaUI/Haskoli.AvaloniaUI.csproj

# Install/refresh this repo as a dotnet template (run from repo root)
dotnet new install .
dotnet new list          # confirm it's registered
dotnet new kasei.arch -o yourNewProjectName
```

There are **no test projects** in the solution (the `tests` solution folder is an empty placeholder) — don't assume a test command exists; check with the user before adding a test framework.

The SDK is pinned via `global.json` to `10.0.301` (`rollForward: latestMinor`). Verify with `dotnet --list-sdks` if build issues look version-related.

### EF Core migrations

Two separate `DbContext`s, each with its own migrations flow, both run with `Haskoli.Api` as the startup project:

```bash
# App data (HaskoliDbContext) — from src/Haskoli.Infrastructure.Persistence
dotnet ef migrations add <Name> --context HaskoliDbContext --project Haskoli.Infrastructure.Persistence.csproj --startup-project ../Haskoli.Api/Haskoli.Api.csproj --output-dir Data/Migrations
dotnet ef database update --context HaskoliDbContext --project Haskoli.Infrastructure.Persistence.csproj --startup-project ../Haskoli.Api/Haskoli.Api.csproj

# Identity data (HaskoliIdentityDbContext) — from src/Haskoli.Infrastructure.Identity
dotnet ef migrations add <Name> --context HaskoliIdentityDbContext --project Haskoli.Infrastructure.Identity.csproj --startup-project ../Haskoli.Api/Haskoli.Api.csproj --output-dir Data/Migrations
dotnet ef database update --context HaskoliIdentityDbContext --project Haskoli.Infrastructure.Identity.csproj --startup-project ../Haskoli.Api/Haskoli.Api.csproj
```

Use a matching `dotnet-ef` tool version: `dotnet tool update --global dotnet-ef --version 10.0.10`.

### Database provider

Selected via `"Database"` in `src/Haskoli.Api/appsettings.json` (`mssql` is the only working option). **MySQL is intentionally disabled**: `Pomelo.EntityFrameworkCore.MySql` has no EF Core 10 release yet, so the `"mysql"` branch in `HaskoliDbContextFactory`, `Infrastructure.Persistence/ServiceCollection/ServiceExtension`, and `IdentityServiceRegistration` deliberately throws `NotSupportedException` instead of being removed — it's a placeholder to restore once Pomelo ships. Don't "fix" it by deleting the branch; restore the real `UseMySql(...)` call instead when the package is available.

## Architecture

### Project layout and dependency direction

Standard Clean Architecture layering, all projects targeting `net10.0`:

```
Haskoli.Domain                     (entities, DTOs, exceptions, interfaces — no dependencies on other projects)
  ^
Haskoli.Application                (MediatR handlers/validators, AutoMapper profiles, CQRS "Features")
  ^
Haskoli.Infrastructure.Common      (repository/service implementations, helpers)
Haskoli.Infrastructure.Persistence (HaskoliDbContext, EF configs, migrations)
Haskoli.Infrastructure.Identity    (HaskoliIdentityDbContext, ASP.NET Identity, JWT issuing)
Haskoli.Infrastructure.UnitOfWork  (generic BaseRepository, UnitOfWork, DbFactory)
  ^
Haskoli.Api                        (ASP.NET Core host: controllers, middleware, Swagger, Serilog)
Haskoli.AvaloniaUI                 (desktop client — currently a mostly-stock Avalonia scaffold;
                                       only references Application + Domain, not the API/Infrastructure)
```

Everything is wired up through `IServiceCollection` extension methods, one per project, all invoked from `ConfigureServiceExtension.InitConfigurationAPI` (`src/Haskoli.Api/ServiceCollection/ConfigureServiceExtension.cs`), which `Startup.ConfigureServices` calls:
- `AddApplicationLayer()` — AutoMapper, FluentValidation validators, MediatR, `ValidationBehaviour` pipeline
- `AddPersistenceLayer(configuration)` — `HaskoliDbContext` provider selection
- `AddUnitOfWorkLayer()` — generic repository/UoW registrations
- `AddCommonLayer(configuration)` — concrete repositories/services (e.g. `ICountryRepository` → `CountryRepository`), helpers, email settings
- `ConfigureIdentityServices(configuration)` — `HaskoliIdentityDbContext`, ASP.NET Identity, JWT bearer auth

HTTP pipeline order lives in `AppBuilderExtension.InitConfigurationAPI` (`src/Haskoli.Api/ServiceCollection/AppBuilderExtension.cs`): Swagger (dev only) → HTTPS redirect → routing → authN → authZ → CORS (`CorsePolicy`, currently allow-any — a template default, tighten for real deployments) → `ErrorHandlerMiddleware` → endpoints.

### Request flow (the pattern every feature follows)

`Controller` → `IMediator.Send(Query/Command)` → MediatR `Handler` (in `Haskoli.Application/Features/<Entity>/{Commands,Queries}`) → `I<Entity>Service` (`Haskoli.Infrastructure.Common/Services`) → `I<Entity>Repository<HaskoliDbContext>` (generic base in `Haskoli.Infrastructure.UnitOfWork/Repository/Base/BaseRepository.cs`, entity-specific overrides in `Haskoli.Infrastructure.Common/Repositories`) → `HaskoliDbContext`. AutoMapper converts between entities and DTOs at the handler/service boundary.

The `Country` entity under each of these locations is the reference implementation — mirror its shape (interface + implementation pairs, `Commands`/`Queries` folders, validator alongside each command handler) when adding a new entity. Naming conventions (from `docs/README.md`): `I<Entity>Repository` / `<Entity>Repository`, `I<Entity>Service` / `<Entity>Service`, `<Entity>DTO` / `Create<Entity>DTO` / `Update<Entity>DTO` / `Delete<Entity>DTO`, `<Entity>Controller`. New DI registrations for a new entity go in `Haskoli.Infrastructure.Common/ServiceCollection/ServiceCollection.cs` (`AddCommonLayer`); new AutoMapper maps go in `Haskoli.Application/Mappings/AutoMapperProfile.cs`.

### Error handling

Domain/application code throws typed exceptions from `Haskoli.Domain/Exceptions/**` (e.g. `EntityNotFoundException`, `ValidateException`, `BusinessException`, `IdentityException`). `ErrorHandlerMiddleware` (`src/Haskoli.Api/Middleware/ErrorHandlerMiddleware.cs`) is the single place that maps each exception type to an HTTP status code and serializes an `ApiResponse<string>` — add new exception types there when introducing a new failure mode rather than handling status codes in controllers.

### Cross-cutting config

- **Serilog** reads sinks (Console, File, Seq, MSSqlServer) from the `Serilog` section of `appsettings.json`; configured once in `Program.cs` via `UseSerilog`.
- **JWT auth**: settings under `JwtSettings` in `appsettings.json`, issued by `AuthService` (`Haskoli.Infrastructure.Identity/Services/AuthService.cs`), validated via `IdentityServiceRegistration.ConfigureIdentityServices`.
- Controllers use **Newtonsoft.Json** (via `AddNewtonsoftJson`) with camelCase contract resolver, not `System.Text.Json`.
- `appsettings.json` ships with placeholder secrets (JWT key, Seq API key, SendGrid key) — expected to be overridden per deployment/template instantiation, not treated as real secrets.

### Stale build artifacts

`obj/`/`bin/` folders under several projects still contain leftover `net9.0` outputs from before the .NET 10 upgrade (see `openspec/changes/archive/2026-07-19-upgrade-to-net10/`). Current `.csproj` files all target `net10.0` — don't be misled by the `net9.0` paths still present in build output.

## Spec-driven changes (OpenSpec)

This repo uses [OpenSpec](openspec/) for planning non-trivial changes, with matching Cursor slash-commands/skills in `.cursor/commands/opsx-*.md` and `.cursor/skills/openspec-*`. Workflow: `propose` (creates `openspec/changes/<name>/` with `proposal.md`, `design.md`, `tasks.md`) → implement → `archive` (moves the change into `openspec/changes/archive/` and updates `openspec/specs/`). Current specs live under `openspec/specs/<capability>/spec.md` (e.g. `platform-runtime` — target framework, EF Core-only, dependency hygiene requirements established by the .NET 10 upgrade). Check `openspec/specs/` and `openspec/changes/` before starting sizable work to see if a proposal already covers it or if new work should get one.
