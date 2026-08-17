## Context

The Haskoli Clean Architecture template is a 9-project .NET solution:

```
Api (Web) ─┬─ Application ── Domain
           ├─ Infrastructure.Persistence ─┐
           ├─ Infrastructure.Identity     ├─ Domain
           ├─ Infrastructure.Common       │
           └─ Infrastructure.UnitOfWork ──┘
AvaloniaUI ── Application, Domain
template   (packaging project)
```

All projects currently target `net9.0` and pin Microsoft packages at `9.0.8`. Investigation confirmed:

- **EF Core is the sole ORM** — real `DbContext`/`IdentityDbContext<ApplicationUser>` types, `UseSqlServer`/`UseMySql`, and `dotnet ef` migrations. There are **zero** `System.Data.Entity` (EF6) references.
- The legacy `EntityFramework 6.5.1` package is nonetheless referenced in `Infrastructure.Persistence`, `Infrastructure.Common`, and `Infrastructure.UnitOfWork` — dead weight.
- EF provider packages (Sqlite, SqlServer, Pomelo MySql, Design, Tools) are duplicated across multiple infrastructure projects.
- Some web-only / legacy packages leak into infrastructure layers (`Microsoft.AspNetCore.Mvc.Core 2.3.0`, `Microsoft.AspNetCore.Mvc.NewtonsoftJson`, deprecated `FluentValidation.AspNetCore`).

## Goals / Non-Goals

**Goals:**
- Move all projects to `net10.0` on a supported (LTS) runtime.
- Keep the Microsoft package families version-aligned with the runtime (`10.0.x`).
- Remove the legacy EF6 package and other unused/misplaced dependencies without changing behavior.
- Preserve EF Core as the only ORM and keep migrations working.

**Non-Goals:**
- No refactor of the architecture (e.g., removing the `DbContext` constraint leaking into `Domain` interfaces).
- No functional/feature changes to the API, Identity, or Avalonia UI.
- No database schema changes or new migrations beyond what is needed to verify the upgrade.

## Decisions

- **Retarget in lockstep, not incrementally.** Change `TargetFramework` in all 9 projects together and bump Microsoft packages in the same pass. Rationale: EF Core / ASP.NET Core / Extensions packages must match the runtime major version; mixing 9.x and 10.x causes runtime binding failures. Alternative (upgrade project-by-project) rejected because shared `Domain`/`Application` references force a big-bang anyway.
- **Remove EF6 outright rather than isolate it.** Since there are no `System.Data.Entity` usages, deletion is safe with no shim needed. Alternative (keep for safety) rejected — it only adds confusion between EF6 and EF Core.
- **Consolidate EF providers to the persistence-owning projects.** Declare Sqlite/SqlServer/Pomelo/Design/Tools only where a `DbContext` or migrations live (`Infrastructure.Persistence`, `Infrastructure.Identity`, and `Api` as startup). Remove them from projects that only reference `DbContext` abstractly. Rationale: reduces transitive bloat and version drift.
- **Verify third-party compatibility before pinning.** `Pomelo.EntityFrameworkCore.MySql`, `Utilities.Core.dll`, and Avalonia/SkiaSharp are checked against .NET 10 and pinned to the lowest compatible stable version.
- **Park MySQL (Pomelo) support (decided during apply).** NuGet verification confirmed the latest `Pomelo.EntityFrameworkCore.MySql` is `9.0.0`, hard-capped to `Microsoft.EntityFrameworkCore.Relational [9.0.0, 9.0.999]` — there is no EF Core 10 release. Since EF Core resolves to a single version solution-wide, MySQL cannot coexist with EF Core 10. Decision: proceed to EF Core `10.0.x` now and temporarily remove the Pomelo package references and the `mysql` provider branches (replaced with a clear "not supported pending Pomelo EF Core 10" error). SQL Server and SQLite remain fully supported. Restoring MySQL is tracked as a follow-up for when Pomelo ships an EF Core 10 release. Alternative (stay on EF Core 9 / runtime-only retarget) was considered and rejected by the maintainer in favor of moving fully to EF Core 10.
- **Introduce `platform-runtime` capability spec** to make the runtime baseline and ORM policy an explicit, testable requirement rather than tribal knowledge.

## Risks / Trade-offs

- **Pomelo MySQL lag** → Pomelo has historically shipped its EF-Core-major release after GA. Mitigation: verify a net10-compatible Pomelo version exists before starting; if none, temporarily keep the current provider on a compatible EF Core version or gate MySQL support behind a follow-up.
- **`Utilities.Core.dll` compatibility unknown** → a custom `.dll`-named package may not have a net10 build. Mitigation: confirm it loads under net10.0 (netstandard2.0 packages generally work); flag as blocker if it fails.
- **Removing a package that is transitively relied upon** → cleanup could break a hidden usage. Mitigation: remove incrementally and rely on a clean `dotnet build` + migration smoke test after each removal batch.
- **SDK availability on build agents** → net10.0 requires the .NET 10 SDK everywhere. Mitigation: document the SDK requirement; optionally add a `global.json` to pin the SDK band.

## Migration Plan

1. Ensure the .NET 10 SDK is installed locally and on CI.
2. Retarget all `.csproj` files to `net10.0`.
3. Bump Microsoft package families to `10.0.x`.
4. Remove legacy EF6 and redundant/misplaced packages; consolidate EF providers.
5. Verify third-party package compatibility and pin versions.
6. `dotnet restore` + `dotnet build` the full solution; resolve breaks.
7. Run an EF Core migration smoke test for both `HaskoliDbContext` and `HaskoliIdentityDbContext`.
8. Rollback strategy: the change is confined to `.csproj` files and package versions — revert the commit to restore the net9.0 state.

## Open Questions

- Which exact `10.0.x` patch should be pinned (latest stable at implementation time)?
- Is there a net10-compatible `Pomelo.EntityFrameworkCore.MySql` release, or must MySQL support be temporarily deferred?
- Does `Utilities.Core.dll` have a build that loads under net10.0?
