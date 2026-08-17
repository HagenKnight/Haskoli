## 1. Prerequisites & Compatibility Verification

- [x] 1.1 Install/confirm the .NET 10 SDK locally (`dotnet --list-sdks`) and note the version to pin — SDK `10.0.301` present; EF Core stable is `10.0.10`
- [x] 1.2 Verify a .NET 10 / EF Core 10 compatible `Pomelo.EntityFrameworkCore.MySql` release exists; record the target version — **BLOCKER: none exists.** Latest Pomelo is `9.0.0`, which hard-caps `Microsoft.EntityFrameworkCore.Relational [9.0.0, 9.0.999]`. MySQL is actively used (`UseMySql`, `MySqlOptionsExtension`, `HaskoliDbContextFactory`), so EF Core cannot move to 10.x while Pomelo is referenced.
- [x] 1.3 Verify `Utilities.Core.dll` (in `Haskoli.Application`) loads/builds under `net10.0`; flag as blocker if not — restores & builds fine (to be re-confirmed on the net10 build)
- [x] 1.4 Confirm Avalonia (11.3.x) and SkiaSharp (3.119.x) run on `net10.0` (no version change expected) — framework-independent; build clean
- [x] 1.5 Establish a baseline: `dotnet build` the solution and record current warnings/errors — baseline: 0 errors, 135 pre-existing warnings

## 2. Retarget Framework to net10.0

- [x] 2.1 Set `TargetFramework` to `net10.0` in `Haskoli.Domain.csproj`
- [x] 2.2 Set `TargetFramework` to `net10.0` in `Haskoli.Application.csproj`
- [x] 2.3 Set `TargetFramework` to `net10.0` in `Haskoli.Api.csproj`
- [x] 2.4 Set `TargetFramework` to `net10.0` in `Haskoli.Infrastructure.Persistence.csproj`
- [x] 2.5 Set `TargetFramework` to `net10.0` in `Haskoli.Infrastructure.Identity.csproj`
- [x] 2.6 Set `TargetFramework` to `net10.0` in `Haskoli.Infrastructure.Common.csproj`
- [x] 2.7 Set `TargetFramework` to `net10.0` in `Haskoli.Infrastructure.UnitOfWork.csproj`
- [x] 2.8 Set `TargetFramework` to `net10.0` in `Haskoli.AvaloniaUI.csproj`
- [x] 2.9 Update `template/TemplatePack.csproj` and any `.template.config` framework references to net10.0 — TemplatePack retargeted; `.template.config/template.json` has no framework pin
- [x] 2.10 Add a `global.json` pinning the .NET 10 SDK band for reproducible builds — created at repo root (`version 10.0.301`, `rollForward latestMinor`); `dotnet --version` resolves `10.0.301`

## 3. Bump Runtime-Aligned Microsoft Packages to 10.0.x

- [x] 3.1 Bump all `Microsoft.EntityFrameworkCore.*` (Core, Design, Tools, SqlServer) to `10.0.10` — Sqlite dropped (unused, no `UseSqlite`)
- [x] 3.2 Bump all `Microsoft.AspNetCore.*` (JwtBearer, Identity.EntityFrameworkCore, Mvc.NewtonsoftJson) to `10.0.10`; dropped legacy `Microsoft.AspNetCore.Identity 2.3.1` and unused `Microsoft.AspNetCore.OpenApi`
- [x] 3.3 Bump all `Microsoft.Extensions.*` (Configuration.*, Options.ConfigurationExtensions, DependencyInjection.Abstractions) to `10.0.10`
- [x] 3.4 ~~Update `Pomelo.EntityFrameworkCore.MySql`~~ — superseded by Group 4b (park MySQL; no EF Core 10 release exists)
- [x] 3.5 Verify Serilog, AutoMapper, MediatR, FluentValidation, Swashbuckle, SendGrid versions are compatible — all build; bumped `System.IdentityModel.Tokens.Jwt` to `8.19.2` (required by JwtBearer 10). `AutoMapper` bumped `14.0.0 → 16.2.0` to fix advisory GHSA-rvv3-g6hj-g44x (updated `AddAutoMapper` to the config-based `cfg => cfg.AddMaps(...)` API). `dotnet list package --vulnerable` now reports zero vulnerable packages across all 7 backend projects (only Avalonia's transitive `Tmds.DBus.Protocol` remains, outside our control).

## 4b. Park MySQL (Pomelo) Support — pending Pomelo EF Core 10

- [x] 4b.1 Remove `Pomelo.EntityFrameworkCore.MySql` package from all projects (`Api`, `Persistence`, `Common`, `UnitOfWork`)
- [x] 4b.2 Guard the `mysql` provider branches (throw `NotSupportedException`) in `HaskoliDbContextFactory`, Persistence `ServiceExtension`, and Identity registration
- [x] 4b.3 Remove Pomelo type usages from `HaskoliDbContext` and `HaskoliIdentityDbContext` provider-detection logic
- [x] 4b.4 Add a follow-up note (README/docs) to restore MySQL when Pomelo ships an EF Core 10 release
- [x] 4b.5 Remove unused `Serilog.Sinks.MySQL` (not in Serilog `Using` config) from `Api` and `AvaloniaUI` — dragged EOL Pomelo 5.0.1

## 4. Remove Legacy EF6 (EF-NetFramework)

- [x] 4.1 Remove `EntityFramework` `6.5.1` from `Haskoli.Infrastructure.Persistence.csproj`
- [x] 4.2 Remove `EntityFramework` `6.5.1` from `Haskoli.Infrastructure.Common.csproj`
- [x] 4.3 Remove `EntityFramework` `6.5.1` from `Haskoli.Infrastructure.UnitOfWork.csproj`
- [x] 4.4 Re-confirm there are no `using System.Data.Entity` references anywhere after removal — confirmed, zero matches

## 5. Remove Unused / Misplaced Packages

- [x] 5.1 Remove `Microsoft.AspNetCore.Mvc.Core` `2.3.0` from `Infrastructure.Persistence` and `Infrastructure.UnitOfWork` (no MVC usage in infra)
- [x] 5.2 Remove `Microsoft.AspNetCore.Mvc.NewtonsoftJson` from non-web infrastructure projects (kept only in `Api`); added plain `Newtonsoft.Json 13.0.3` to `Common` (used by `JsonHelper`)
- [x] 5.3 Remove deprecated `FluentValidation.AspNetCore` (unused); added `FluentValidation.DependencyInjectionExtensions 12.0.0` to `Application` for `AddValidatorsFromAssembly`
- [x] 5.4 Remove `Microsoft.VisualStudio.Web.CodeGeneration.Design` from `Haskoli.Api.csproj`
- [x] 5.5 Verify `AutoMapper`/`MediatR` in `Haskoli.Domain`: removed unused `AutoMapper`, kept `MediatR` (DTOs implement `IRequest`) and `Microsoft.EntityFrameworkCore`; added `AutoMapper` directly to `Application` (was transitive via Domain)
- [x] 5.6 Remove unused custom package `Utilities.Core.dll` from `Application` — it dragged EOL/vulnerable transitives (Pomelo 5.0.1, SixLabors.ImageSharp 1.0.0, System.Data.SqlClient, System.Drawing.Common); no `Utilities` usage in code and build stays green

## 6. Consolidate EF Provider Packages

- [x] 6.1 Determine which projects own a `DbContext`/migrations (`Persistence`, `Identity`) and the startup project (`Api`)
- [x] 6.2 Keep `SqlServer` in `Persistence`+`Identity`; `Design`/`Tools` in `Persistence`+`Identity`+`Api` (startup); dropped `Sqlite` everywhere (unused)
- [x] 6.3 Remove duplicated EF provider references from `Common`/`UnitOfWork` (only use `DbContext` abstractly; providers come transitively via project refs)
- [x] 6.4 Rebuild to confirm no missing-provider errors — build green; both DbContexts resolve `Microsoft.EntityFrameworkCore.SqlServer` at design time

## 7. Build & Verify

- [x] 7.1 `dotnet restore` the full solution and resolve any package resolution errors — resolved (fixed NU1605 JWT downgrade, NU1608 Pomelo transitive)
- [x] 7.2 `dotnet build` the full solution — 0 errors. Remaining warnings are pre-existing nullability + advisory NU1903 for `AutoMapper 14.0.0` and Avalonia's transitive `Tmds.DBus.Protocol`
- [x] 7.3 Design-time load of `HaskoliDbContext` via `dotnet ef dbcontext info` (SqlServer). NOTE: full add/update against a live DB requires a SQL Server instance (deferred to user environment)
- [x] 7.4 Design-time load of `HaskoliIdentityDbContext` via `dotnet ef dbcontext info` (SqlServer). NOTE: full add/update against a live DB deferred to user environment
- [x] 7.5 Launch `Haskoli.Api` and confirm Swagger + Identity/JWT — connection string in `src/Haskoli.Api/appsettings.Development.json` configured and manually tested by maintainer
- [x] 7.6 Launch `Haskoli.AvaloniaUI` on net10.0 — launched successfully by maintainer
- [x] 7.7 Update `docs/README.md` prerequisites to reference .NET 10 — added Prerequisites section + MySQL-parked note

## 8. Validation

- [x] 8.1 Run `openspec validate upgrade-to-net10` and resolve any issues — passed (1/1)
- [x] 8.2 Confirm every `platform-runtime` spec scenario is satisfied — net10 target (all csproj), no EF6 / no `System.Data.Entity`, Microsoft families aligned to `10.0.10`, providers consolidated, unused deps removed
