## ADDED Requirements

### Requirement: Target framework baseline
All projects in the solution SHALL target `net10.0`. The solution MUST build and run on the .NET 10 SDK/runtime.

#### Scenario: All projects target net10.0
- **WHEN** any `.csproj` in the solution is inspected
- **THEN** its `TargetFramework` (or `TargetFrameworks`) SHALL be `net10.0`

#### Scenario: Solution builds on .NET 10
- **WHEN** `dotnet build` is run against the solution with the .NET 10 SDK installed
- **THEN** the build SHALL succeed with no framework-targeting errors

### Requirement: EF Core is the sole ORM
The solution SHALL use Entity Framework Core as its only object-relational mapper. The legacy `EntityFramework` (EF6 / .NET Framework-era) package MUST NOT be referenced by any project.

#### Scenario: No legacy EntityFramework package
- **WHEN** the package references of every project are inspected
- **THEN** no project SHALL reference the `EntityFramework` package (version 6.x)

#### Scenario: No EF6 namespace usage
- **WHEN** the source code is searched for `using System.Data.Entity`
- **THEN** no matches SHALL be found

#### Scenario: EF Core data access continues to work
- **WHEN** an EF Core migration is applied for `HaskoliDbContext` or `HaskoliIdentityDbContext` after the upgrade
- **THEN** the migration SHALL complete successfully against a supported provider

### Requirement: Runtime-aligned package versions
Package families whose versions are coupled to the runtime — `Microsoft.EntityFrameworkCore.*`, `Microsoft.AspNetCore.*`, and `Microsoft.Extensions.*` — SHALL be pinned to the `10.0.x` band consistently across all projects.

#### Scenario: Microsoft package families aligned to 10.0.x
- **WHEN** references to `Microsoft.EntityFrameworkCore.*`, `Microsoft.AspNetCore.*`, or `Microsoft.Extensions.*` are inspected across projects
- **THEN** each SHALL resolve to a `10.0.x` version and MUST NOT mix `9.x` with `10.x`

### Requirement: No unused or misplaced dependencies
Each project SHALL reference only the packages it actually uses, and web/presentation-only packages MUST NOT be referenced by infrastructure or domain layers.

#### Scenario: Removed dead dependencies
- **WHEN** the project files are inspected after the upgrade
- **THEN** packages identified as unused (e.g., legacy `EntityFramework`, `FluentValidation.AspNetCore`, `Microsoft.AspNetCore.Mvc.Core` in infrastructure, scaffolding-only design packages) SHALL be removed

#### Scenario: EF providers declared only where needed
- **WHEN** EF provider packages (`Sqlite`, `SqlServer`, `Pomelo.EntityFrameworkCore.MySql`, `Design`, `Tools`) are inspected
- **THEN** they SHALL be declared only in the persistence-owning projects and the startup (`Api`) project, not duplicated across every infrastructure project
