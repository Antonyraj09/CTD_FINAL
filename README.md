# CTD_FINAL — CTD Management System

A production-grade ASP.NET Core 8 MVC + SQL Server application for managing CTD (customs transit declaration) jobs — India–Nepal transit permits — end to end: job creation through CTD generation, tracking, delivery, billing and document archival, plus master data, users/roles, audit history, reporting, and system-generated alerts.

This is a single-project restructure of the original multi-project [CtdSuite](../CTD_NEW) Clean Architecture solution — same business logic, same screens and behavior, reorganized into one ASP.NET Core MVC project with folders instead of separate Core/Infrastructure/Shared class libraries. One deliberate schema difference from CtdSuite: Importer, Agent and Transporter are unified into a single **Party** master (`Entities/Party.cs`) with `IsImporter`/`IsTransporter`/`IsAgent` role flags, so one company record can hold any combination of roles instead of needing a separate record — and separate identity — in three different tables.

### Party Master

Party has its own dedicated screen (`Controllers/PartyController.cs`, `Views/Party/`) rather than the generic Masters modal — it needs a repeatable branches grid, which the generic single-entity Masters CRUD system doesn't support. Fields:

- **Identity**: legal name, trade name, constitution (Proprietorship/Partnership/LLP/Pvt Ltd/...), PAN, IEC code (DGFT Import Export Code), CIN.
- **Roles**: Importer/Transporter/Agent checkboxes (any combination), CHA license + validity (Agent), fleet details (Transporter), AEO trust-status tier + certificate number.
- **Banking**: bank name/account/IFSC, AD (Authorized Dealer) code for forex/shipping-bill declarations.
- **Primary contact**: name, designation, phone, email, website.
- **Branches** (`Entities/PartyBranch.cs`, one-to-many): a party can have any number of branches, each with its own address, city/state/PIN/country, **and its own GSTIN** — GST registration in India is issued per state/place of business, so a party operating from multiple states genuinely needs more than one GSTIN, not just one address field. Each branch also carries phone/email/contact person and an optional local customs-house registration reference. At least one branch is required per party; the first (or whichever is marked) is the primary.

`Data/Migrations/*_MergePartyMaster.cs` and `*_AddPartyBranchesAndDetails.cs` migrate any existing Importer/Agent/Transporter data into the unified table (each becoming its own branch, carrying forward its city/phone/email/GSTIN) and remap `CtdJobs`' existing FK values, so upgrading an installation with real data doesn't lose any job's importer/agent/transporter links. Seed data intentionally stays minimal (legal name, role, one branch) — PAN/IEC/CIN/banking/AEO fields are left blank for real data entry via the UI rather than fabricated.

### Job ISNE is now the primary job record

Tracking, the Dashboard, and Reports were originally built against `CtdJob` (the 4-step Wizard's entity — `Status`/`BillingStatus` enums, a `BorderPoint` FK, a `Containers` collection, service/transport/tax charge fields). They now read from **`JobIsne`** instead (`Entities/JobIsne.cs`, the ~60-field ERP-style job form) — the Wizard/`CtdJob` path is no longer linked from navigation. `JobIsne` has a different, flatter shape than `CtdJob`, so the screens were rebuilt around what it actually has rather than faking the missing concepts:

- **Status**: JobIsne has no `WorkflowStatus` enum, so `Helpers/JobIsneStatus.cs` derives an honest 3-state pseudo-status from the fields that exist — **Pending CTD** (no CTD number yet), **CTD Issued** (`CtdNumber` set, no `VesselArrival`), **Arrived** (`VesselArrival` set).
- **Border point**: JobIsne has no `BorderPoint` FK, only a free-text `RouteOfTransit` field — that's what Tracking's filter, the Dashboard's route-volume chart, and the reports group/display now use.
- **Revenue/billing**: JobIsne has no `ServiceCharge`/`TransportCharge`/`Tax`/`Total`/`BillingStatus`. Dashboard KPIs and the Monthly chart use `SUM(DutyAmount)` labeled "Total Duty" instead of "Revenue"; the "Billing Summary" report is now **Commercial Value Summary** (FOB/Freight/CIF/Duty per job); "Customer-wise Revenue" is now **Customer-wise Duty**; the Customer Dashboard's "Billing Snapshot" is now a **Commercial Value Snapshot** (FOB/CIF/Duty totals, no billed/paid/outstanding since there's nothing to bill against).
- **Customer Dashboard bridging**: JobIsne has no FK to `Party` — only free-text `PartyCode`/`PartyName` — so the Customer Dashboard's `importerId` (a `Party.Id`) is bridged by matching that party's legal name against `JobIsne.PartyName` (`DashboardService.JobsForImporterAsync`), not a real foreign key.
- **Invoice generation** (`JobsController.Invoice`/`InvoicePrint`) has no JobIsne equivalent — JobIsne carries no service/transport/tax charge fields to invoice against — and is left as dead code reachable only from the now-unlinked Wizard, rather than fabricated.
- Document Archive needed no changes: `GeneratedDocument.JobNo` was already a free-text field with no FK to `CtdJob`, so it works with `JobIsne.JobNumber` as-is.

The old `CtdJob`/Wizard/`Jobs/Tracking` code is untouched and still functions — it's simply no longer linked from the sidebar or Dashboard.

### Sub-Agent master

A lightweight master (`Entities/SubAgent.cs`) for the local customs clearing sub-agents referenced by `JobIsne.SubAgentCode`/`SubAgentName` (free text, not an FK — same convention as `JobIsne.PartyName`). It lives in the generic Masters screen (`Controllers/MastersController.cs` + `Infrastructure/Masters/MasterRegistry.cs`, tab key `subagent`) alongside Commodity/Route/Border Point/Customs House rather than a dedicated screen, since — unlike Party — it's a flat record with no branches. Fields: Sub-Agent Code (unique) and Name, Address Line 1/2, City/State/PIN, Customs/CHA License No., PAN, GSTIN, contact person, phone, email. `MastersController.Save` now catches `DbUpdateException` (duplicate code) with a friendly error instead of a 500 — this was a latent gap in the generic Masters CRUD (only `Delete` handled `DbUpdateException` before) surfaced by adding SubAgent's unique code index.

Party (`Entities/Party.cs`) now also carries a `SubAgentCode` — the local sub-agent a party normally clears cargo through — as the same kind of loose code reference (not an FK), picked from a dropdown sourced from the Sub-Agent master (`Views/Party/Edit.cshtml`, Section B).

### Typeable combo-select (all dropdowns)

Every `<select>` in the app is progressively enhanced client-side into a type-to-filter dropdown (`wwwroot/js/combobox.js` + the `.combo-*` rules in `wwwroot/css/site.css`) — type to filter options, click or Enter to pick, Esc/click-away to close. This is a pure UI-layer enhancement: the original `<select>` stays in the DOM (just hidden), so it keeps its `id`, its value, and fires real `change` events — every existing `$("#id").value` read and `addEventListener("change", ...)` across the codebase kept working unmodified. `combobox.js` runs once on page load and is also invoked from `openModal()` (`site.js`) so selects inside AJAX-loaded modals (Users, Masters, Alerts, Documents) get enhanced too. The one gap: a `<select>` whose *options* are replaced by JS after the page loads (not just its value) needs an explicit `refreshCombo(id)` call afterward so the visible label updates — already wired for Tracking's "Clear filters" buttons and the Customer Dashboard's shipment-timeline selector; keep this in mind if a new screen repopulates a select's options via JS. The CTD Job Wizard's per-container "Size" dropdown (rendered dynamically inside a dense table row) was left as a native `<select>` since that screen is no longer linked from navigation.

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
