## Why

The solution targets .NET 9 (`net9.0`) across all 9 projects and carries dependency debt: a legacy `EntityFramework 6.5.1` (the .NET Framework-era EF) package is referenced in three infrastructure projects even though the codebase uses EF Core exclusively, and several packages are duplicated or misplaced across layers. Moving to .NET 10 (LTS) keeps the template on a supported runtime and is the right moment to remove the dead and misplaced dependencies.

## What Changes

- Retarget every project from `net9.0` to `net10.0` (`Domain`, `Application`, `Api`, `Infrastructure.Persistence`, `Infrastructure.Identity`, `Infrastructure.Common`, `Infrastructure.UnitOfWork`, `AvaloniaUI`, and the `template` pack).
- Bump the Microsoft package families that are tied to the runtime to the `10.0.x` band: `Microsoft.EntityFrameworkCore.*`, `Microsoft.AspNetCore.*`, and `Microsoft.Extensions.*`.
- Verify and update third-party packages for .NET 10 compatibility: `Pomelo.EntityFrameworkCore.MySql`, `Utilities.Core.dll`, and the Avalonia/SkiaSharp stack.
- **BREAKING (dependency):** Remove the legacy `EntityFramework 6.5.1` package from `Infrastructure.Persistence`, `Infrastructure.Common`, and `Infrastructure.UnitOfWork` — it is unused (no `System.Data.Entity` references). EF Core remains the sole ORM.
- Remove/relocate unused or misplaced packages: `Microsoft.AspNetCore.Mvc.Core 2.3.0` and `Microsoft.AspNetCore.Mvc.NewtonsoftJson` from infrastructure layers, the deprecated `FluentValidation.AspNetCore`, `Microsoft.VisualStudio.Web.CodeGeneration.Design` from `Api`, and consolidate the duplicated EF provider packages (Sqlite/SqlServer/Pomelo/Design/Tools) so providers are declared only where needed.
- Confirm the solution builds and EF Core migrations still run after the upgrade.

## Capabilities

### New Capabilities
- `platform-runtime`: The target framework baseline and the ORM/dependency policy for the solution (what runtime the projects target, that EF Core is the sole ORM, and which package families must stay version-aligned with the runtime).

### Modified Capabilities
<!-- No existing specs in openspec/specs/; nothing to modify. -->

## Impact

- **Runtime/SDK:** Requires the .NET 10 SDK; CI/build agents and contributor machines must have it installed.
- **Projects:** All 9 `.csproj` files (TargetFramework + package versions).
- **Dependencies:** Removal of legacy EF6 and redundant packages; version bumps across Microsoft package families; third-party compatibility verification.
- **Data layer:** No schema or migration behavior change intended — EF Core stays; migrations must be re-verified after the bump.
- **Risk area:** `Pomelo.EntityFrameworkCore.MySql` and `Utilities.Core.dll` may lag on .NET 10 support and need pinning to compatible versions.
