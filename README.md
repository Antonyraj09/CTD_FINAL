# CTD_FINAL — CTD Management System

A production-grade ASP.NET Core 8 MVC + SQL Server application for managing CTD (customs transit declaration) jobs — India–Nepal transit permits — end to end: job creation through CTD generation, tracking, delivery, billing and document archival, plus master data, users/roles, audit history, reporting, and system-generated alerts.

This is a single-project restructure of the original multi-project [CtdSuite](../CTD_NEW) Clean Architecture solution — same business logic, same screens and behavior, reorganized into one ASP.NET Core MVC project with folders instead of separate Core/Infrastructure/Shared class libraries. One deliberate schema difference from CtdSuite: Importer, Agent and Transporter are unified into a single **Party** master (`Entities/Party.cs`) with `IsImporter`/`IsTransporter`/`IsAgent` role flags, so one company record can hold any combination of roles instead of needing a separate record — and separate identity — in three different tables. `Data/Migrations/*_MergePartyMaster.cs` migrates any existing Importer/Agent/Transporter data into the unified table and remaps `CtdJobs`' existing FK values, so upgrading an installation with real data doesn't lose any job's importer/agent/transporter links.

## Project layout

```
CTD_FINAL.sln
CTD_FINAL.csproj
Program.cs
Constants/        Role names, permission keys
Controllers/      MVC controllers
Data/             AppDbContext, Fluent API configurations, EF Core migrations, DB seeding
DTOs/             Cross-layer data transfer objects (dashboard, reports, jobs, paging)
Entities/         Domain entities + ASP.NET Core Identity user/role
Enums/            Workflow status, billing status, alert channel, etc.
Helpers/          Password generator and other small utilities
Infrastructure/   Web-layer cross-cutting: permission-based authorization, Identity claims factory,
                  the Masters screen's tab registry, global exception middleware
Interfaces/       Service/repository contracts
Models/           MVC view models / request DTOs bound from forms and AJAX calls
Repositories/     Generic EF Core repository
Services/         Business logic (jobs, documents, dashboard, reports, alerts, audit, permissions...)
Views/            Razor views
wwwroot/          CSS, JS, client libraries (restored via libman.json)
database/scripts/ Idempotent SQL script generated from the EF Core migration
```

- **Data access**: EF Core 8 Code-First against SQL Server, migrated at startup (`Database.MigrateAsync()`).
- **Auth**: ASP.NET Core Identity (`ApplicationUser`/`ApplicationRole`, int keys), cookie authentication. Authorization is driven by a DB-seeded Role Permission Matrix (`RolePermission` table) — a `[RequirePermission(key)]` filter checks the current user's role(s) against that table (cached 10 minutes) and redirects to Access Denied on failure, rather than static `[Authorize(Roles=...)]`.
- **Alerts**: a pluggable `IAlertSender`; the shipped implementation (`LoggedAlertSender`) logs the alert instead of sending real email/SMS.

## Prerequisites

- .NET 8 SDK
- SQL Server 2019+ (local instance, LocalDB, or `mcr.microsoft.com/mssql/server:2022-latest` via Docker)

## First-time setup

1. **Restore client-side libraries**:
   ```bash
   dotnet tool install -g Microsoft.Web.LibraryManager.Cli   # once
   libman restore
   ```

2. **Point the connection string at your SQL Server.** `appsettings.Development.json` ships targeting a local SQL Server Express instance via Windows auth:
   ```json
   { "ConnectionStrings": { "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CtdFinalDb_Dev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" } }
   ```
   Adjust `.\SQLEXPRESS` if your instance has a different name (check via `sqlcmd -L` or the `SQL Server (...)` entry in `services.msc`). Using a Docker container or a SQL-auth login instead? Swap in:
   ```json
   { "ConnectionStrings": { "DefaultConnection": "Server=localhost,1433;Database=CtdFinalDb_Dev;User Id=sa;Password=<your-password>;TrustServerCertificate=True;MultipleActiveResultSets=true" } }
   ```
   `appsettings.Production.json` / `appsettings.Testing.json` ship with placeholder credentials — override via environment variables or a secrets manager.

   **Note on environments**: the app only reads `appsettings.Development.json` when `ASPNETCORE_ENVIRONMENT=Development`. `Properties/launchSettings.json` sets this automatically for `dotnet run` / Visual Studio F5 (`http`/`https` profiles) — without it, or if you run the published output directly, the app falls back to `Production` and uses the base `appsettings.json` (LocalDB) instead.

3. **Apply migrations and seed data.** Happens automatically on startup. To provision manually:
   ```bash
   sqlcmd -S <server> -d CtdFinalDb -i database/scripts/01_InitialCreate.sql
   # or
   dotnet tool install -g dotnet-ef
   dotnet ef database update
   ```

4. **Run**:
   ```bash
   dotnet run --project CTD_FINAL.csproj
   ```

## Seeded demo accounts

All seeded with the temporary password `ChangeMe#2026`:

| Email | Role |
|---|---|
| `admin@ctdsuite.com` | Administrator |
| `manager@ctdsuite.com` | Manager |
| `operator@ctdsuite.com` | Operator |
| `viewer@ctdsuite.com` | Viewer |

## Adding a schema migration

```bash
dotnet ef migrations add <Name>
dotnet ef migrations script -o database/scripts/01_InitialCreate.sql --idempotent
```

## Security notes

- Every mutating (`POST`) action validates the anti-forgery token; AJAX JSON calls supply it via the `RequestVerificationToken` header.
- All authorization is enforced server-side via `[RequirePermission]`, independent of the (also permission-gated) navigation UI.
- All data access goes through EF Core's parameterized LINQ — no raw/interpolated SQL.
- Uploaded documents are stored under `App_Data/uploads/documents`, outside `wwwroot`, so the static file middleware can never serve them directly; every download is re-authorized through `Documents/Download`. Uploads are restricted to a fixed extension allow-list and a 20 MB size cap.
- Identity passwords require 8+ characters with a digit, uppercase letter and non-alphanumeric character; accounts lock out for 10 minutes after 5 failed attempts.
- Cookies are `HttpOnly`, `SameSite=Lax`; `UseHttpsRedirection`/`UseHsts` are enabled; baseline security response headers (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`) are set on every response.
