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

Every `<select>` in the app is progressively enhanced client-side into a type-to-filter dropdown (`wwwroot/js/combobox.js` + the `.combo-*` rules in `wwwroot/css/site.css`) — type to filter options, click or Enter to pick, Esc/click-away to close. This is a pure UI-layer enhancement: the original `<select>` stays in the DOM (just hidden), so it keeps its `id`, its value, and fires real `change` events — every existing `$("#id").value` read and `addEventListener("change", ...)` across the codebase kept working unmodified. `combobox.js` runs once on page load and is also invoked from `openModal()` (`site.js`) so selects inside AJAX-loaded modals (Users, Masters, Alerts, Documents) get enhanced too. The one gap: a `<select>` whose *options* are replaced by JS after the page loads (not just its value) needs an explicit `refreshCombo(id)` call afterward so the visible label updates — already wired for Tracking's "Clear filters" buttons, the Customer Dashboard's shipment-timeline selector, and the Job ISNE form's Branch selector (below); keep this in mind if a new screen repopulates a select's options via JS. The CTD Job Wizard's per-container "Size" dropdown (rendered dynamically inside a dense table row) was left as a native `<select>` since that screen is no longer linked from navigation.

### Job ISNE: Party Code cascade + field constraints

Party (`Entities/Party.cs`) now also has a `PartyCode` (required going forward in the Party Edit UI, nullable at the DB level so pre-existing rows keep loading; unique, filtered index). The Job ISNE form's Party Code field (`Views/JobIsne/Index.cshtml`) is a dropdown sourced from the Party master instead of free text. Picking a party auto-fills Party Name, Sub-Agent Code/Name (via `Party.SubAgentCode` → Sub-Agent master lookup), and Address:

- One branch on file → its address fills in directly.
- More than one branch → a "Branch" dropdown appears (defaulting to the primary branch) so the user picks which branch's address to pull in.

This only fires on a user-initiated change of Party Code — an existing job's saved Party Name/Address/Sub-Agent fields are left exactly as stored when the page loads, even if the party master has since changed, so opening an old job never silently rewrites its data. The lookup itself is entirely client-side against a JSON blob the controller embeds in the page (`partyLookup`/`subAgentLookup` in `JobIsneController.Index`) — JobIsne has no FK to Party (same free-text-tag convention as `PartyName`/`SubAgentCode` elsewhere), so there's no server round-trip on selection.

Other Job ISNE field changes in this pass:
- **CTD Number**: capped at 25 alphanumeric characters, special characters stripped as you type (`job-isne.js`) and re-validated server-side (`JobIsneController.Save`) since the client filter can be bypassed.
- **Vessel Name** / **Transshipment Vessel**: capped at 30 characters (was 100).
- **Country of CGN** / **Country of Origin**: changed from a fixed 11-country dropdown to free text (was `StringLength(4)`, sized for 2-letter ISO codes — widened to 100 now that it holds full country names).
- **Route of Transit**: changed from free text to a dropdown sourced from the Transit Route master (`TransitRoute.Name`).
- **ROT Date** and **Inward Date**: new fields, added next to ROT Number.
- **Container Number**: capped at 15 alphanumeric characters, same strip-as-you-type + server-side re-check pattern as CTD Number.
- **Miscellaneous Description auto-fill**: switching Container Status to FCL sets it to `SHIPPER'S LOAD & COUNT`; switching to LCL clears it. Only fires on a user-initiated status change (not on page load), so an existing job's custom Misc Description isn't silently overwritten when the form opens with FCL already selected.
- **Due — LOA / Due — Certificate of Origin / Due — Proforma Invoice**: three new date fields in Transit & Delivery Details, alongside the existing Due-date fields (Packing List/Invoice/Original B/L/Insurance Certificate/LC Copy).

### CTD Submission checklist (Word) and Customs Transit Declaration (print)

`JobIsneController.CtdSubmission(id)` / `Views/JobIsne/CtdSubmission.cshtml` — the "ANNEXURE - A" data sheet — is downloaded as a Word document rather than opened for browser printing: the controller sets `ViewResult.ContentType = "application/msword"` and a `Content-Disposition: attachment; filename=...doc` header, and the view carries Word-compatible markup (`xmlns:w`, the `<!--[if gte mso 9]>` `w:WordDocument` block) so Word opens it cleanly — no OOXML library, no new dependency, just HTML Word already knows how to import. Its exact field set, line order and styling were built directly against a reference `.docx` supplied for this feature (inspected at the XML level — `word/document.xml` run-by-run — not just visually, to get every field and every color exactly right): every fetched/database-backed value renders in red (`.val { color: #FF0000; }`), every label stays black, matching the reference's own convention. CHA Code/Name/signatory now come from a `SubAgent` lookup by `SubAgentCode` (not just JobIsne's own free-text `SubAgentName`) — the same lookup `CtdDeclaration` below already does, factored into a shared `LoadAgentAsync` helper. Fields the reference format asks for with no JobIsne equivalent — Importer's PAN, Insurance Policy No/Date/Expiry, Anti-Dumping Duty, Vehicle/Chassis/Engine No. — stay as blank fill-in lines for manual completion, same convention the paper form itself uses; "MARK AND NO" falls back to the literal "N/M" (No Marks) when no container carries a Marks & Serial value, matching real-world CTD paperwork convention. Font is Times New Roman at 10pt and every top-level line is numbered 1–21, both matching the reference exactly — the numbering wasn't visible as plain text in the reference's XML (it's Word's automatic list numbering, defined in `numbering.xml` and only resolvable by cross-referencing each paragraph's `w:numPr`/`w:numId` against that file's `w:start` values, not by extracting `<w:t>` runs alone) — multi-line items (e.g. Importer's Name & Address, or a job with more than one container) keep their continuation lines unnumbered and indented under the parent item's number, same as the reference.

`JobIsneController.CtdDeclaration(id)` / `Views/JobIsne/CtdDeclaration.cshtml` is a new report: the official CHA-issued "Customs Transit Declaration (Import) ICCD" form, replicated as a bordered grid layout matching the reference paper format field-for-field, opened for browser printing (`window.print()`, same convention as every other print view — print-to-PDF is the browser's own dialog, no server-side PDF generation). Available from both the Job ISNE screen and a CTD Tracking row action, alongside (not replacing) the checklist. Field mapping: CHA header block (name, C.H. Agent No., address, phone, GSTIN) ← a `SubAgent` lookup by `JobIsne.SubAgentCode` (not just the free-text `SubAgentName` JobIsne itself carries); HMO' & Import Licence box ← Bank Name + LC No/Date; Ship's Name ← Vessel Name + Voyage No; B/L No. & Date ← Master B/L, falling back to House B/L only if no Master B/L is on file (confirmed against the CTD Submission checklist's reference format, which shows the Master B/L number in this exact slot); the cargo grid's per-container "Marks and Serial No." and container-size/package lines iterate `JobIsneContainer`, while Gr./Net Weight, CIF and Market Value come from JobIsne's own job-level fields; the FOB/Freight/Insurance/CIF computation block is formatted to match the reference form's exact wording, INR values grouped Indian-style (lakhs/crores) via the `en-IN` culture. "Statement as to transportation," the "Org/Dup/..." copy-type line, and "Notify or delivery address" have no JobIsne equivalent and are rendered as static text, same blank-fill-in convention as the checklist.

### Job ISNE: multi-container grid

Section D of the Job ISNE form ("Container & Cargo Details") originally held one set of container fields per job (`ContainerNo`/`ContainerSize`/`ContainerStatus`/`NoPackages`/`CustomsCode`/`MarksSerial`). It's now a **Shipment Information** panel (overall Shipment Type + a "No. of Containers" count + Generate Containers button) plus an editable `JobIsneContainers` grid, so one job can carry any number of containers — or none, for LCL cargo that isn't containerized. `MiscDescription`/`CargoDescription` stay on the job as shared **Common Cargo Details**; `MarksSerial`/`CustomsCode`/weights/packages/package type move to per-row fields, since different containers on the same job can genuinely carry different cargo. Persistence follows the same collection-replace pattern already used for `CtdJob`↔`JobContainer`: on save, the job's existing container rows are removed and rebuilt from whatever's currently in the grid.

- The grid's `<select>` cells (Size, Shipment Type, weight units) are freshly created on every structural re-render (Generate/Add/Delete/Import/Clear), so `renderContainerRows()` calls `enhanceSelects(containerTbody)` afterward — same convention as `openModal()` — to pick up the typeable combo-select behavior described above.
- **Container Number** keeps the existing 15-alphanumeric-character constraint (client-side strip-as-you-type + server-side re-check), now applied per row instead of once per job.
- **"Import from Excel" accepts a CSV file**, not a binary `.xlsx` — this project doesn't vendor a client-side spreadsheet parser, and adding one just for this was judged unnecessary risk/weight for what the feature needs. A CSV with the grid's column headers (`ContainerNo,ContainerSize,ShipmentType,NoPackages,PackageType,GrossWeight,GrossWeightUnit,NetWeight,NetWeightUnit,MarksSerial,CustomsCode`) round-trips through any spreadsheet app's "Save/Export as CSV."
- A row-selection checkbox column (with header select-all) was added to support "Remove Selected" bulk deletion — not shown in the reference mockup, but needed for that action to mean anything with more than one row selected at a time.
- The FCL→`SHIPPER'S LOAD & COUNT` Misc Description auto-fill (only fires on a user-initiated Shipment Type change, never on page load) now hangs off the job-level Shipment Type radio instead of the old per-container Container Status radio — same behavior, just following the field that moved.
- `ReportService.ContainerMovementAsync` now emits one row per container (`SelectMany`) instead of one row per job; `CtdRegisterAsync`'s Customs Code column lists the distinct codes across a job's containers. `DashboardService`'s recent-jobs/customer-shipments container columns switch on container count (0 → "—", 1 → that container's number, N → "N containers"), matching CTD Tracking's column.
- The migration (`CombineJobIsneContainerGrid`) was hand-adjusted after `dotnet ef migrations add`: EF's default diff renamed `NoPackages`→`ShipmentType` (both `int` columns in the same ordinal position) instead of preserving the old `ContainerStatus` (FCL/LCL) column's data — fixed so `ContainerStatus` is renamed into the new job-level `ShipmentType` and `NoPackages` is genuinely dropped, so upgrading a database with real data doesn't silently reinterpret package counts as enum values.

### Job ISNE: "Entry for Data Sheet" section

A new collapsible section (`Views/JobIsne/Index.cshtml`, Section E — the old Section E "Commercial Information" shifted to Section F) captures fields the Nepal CTD Submission ("ANNEXURE-A") paper form asks for that nothing upstream of it ever collected, so those lines no longer have to be filled in by hand after printing:

- All fields in this section are **optional** — none block Save if left blank. Where a field has a fixed format (Importer Code, CIF Value), that format is only enforced when a value is actually entered; an empty field never fails validation.
- **Importer Code**: one fully-editable 6-character field — 2 letters followed by 4 numeric digits (`JobIsne.ImporterCode`, `StringLength(6)`, `RegularExpression(@"^([A-Za-z]{2}\d{4})?$")` — matches empty or a full valid code), validated both client-side (`#isne_importerCode` strips non-alphanumeric characters and caps at 6 as you type, same convention as CTD/Container Number) and server-side (`JobIsneController.Save`, since the client filter can be bypassed). A new job pre-fills the first two characters as `NP` as a convenience default, but they're ordinary editable text, not a locked prefix — the user can type over them entirely.
- **Invoice Number** (20 chars, alphanumeric), **Invoice Date** (date picker), **Certificate of Origin** (30 chars — a distinct field from the existing "Country of Origin," which is the country name, not the certificate reference), **Certificate of Origin Date** (date picker) — all optional, `JobIsne`'s columns are nullable.
- **Sensitive Cargo**: a Yes/No toggle (same `.toggle-switch` control as Green CTD elsewhere on this form), default No. Switching it to Yes reveals **Insurance Company Name & Address** (200-char multi-line text) and **CIF Value** (positive decimal, format-checked only when provided) — both optional even while Sensitive Cargo is Yes, both hidden while it's No. Turning it back off clears any previously entered values on save, so stale insurance data doesn't linger silently against a job that's no longer flagged sensitive. The CIF Value here is intentionally a separate field (`SensitiveCifValue`) from Section F's existing `CifFc` (the shipment's CIF-in-foreign-currency commercial figure) — they answer different questions and can legitimately hold different numbers.
- The section itself is collapsible — a small, generic addition (`.erp-section.collapsible`, a chevron that rotates, click-to-toggle on the header) since no other Section A–F header had this before; the other sections were left as plain (non-collapsible) headers rather than retrofitting a behavior nobody asked for onto them.
- Not wired up in this pass: the CTD Submission checklist still leaves its own Invoice No/Date, Certificate of Origin date, and Insurance Company Name/Address lines blank for manual fill-in, even though Section E now captures equivalent data. Pointing those blanks at the new fields would be a natural follow-up but was left alone since it wasn't asked for here and touches an already-shipped layout.

### Job ISNE: follow-up fixes

- **FCL default Misc Description**: the "SHIPPER'S LOAD & COUNT" auto-fill only ran on a user-initiated Shipment Type change, so a brand-new job opened with FCL already selected (the default) showed no text until the user toggled to LCL and back. `job-isne.js` now applies the default once on load too, but only for a new job (`recordId === 0`) with nothing typed yet — an existing job's saved Misc Description (even if intentionally blank) is still never touched on load.
- **No. of Containers not reflecting manual grid edits**: that field only updated when clicking Generate Containers. `renderContainerRows()` now syncs it to `containerRows.length` on every render, so Add Container/Delete Row/Remove Selected/Import/Clear All all keep it accurate too.
- **Dropdown menus clipped at the bottom of a section**: `.erp-section` had `overflow: hidden` (originally just to crop the header's square corners to match the section's rounded border), which also clipped any typeable combo-select's popup menu when it opened below a field near a section's bottom edge. Fixed by moving the corner-rounding onto `.erp-section-head` directly (`border-top-left/right-radius`) and dropping `overflow: hidden` from `.erp-section` itself — the section's visual appearance is unchanged, but dropdowns near the end of a section are no longer cropped.
- **Save vs. Update button label**: both Save buttons (header and footer) now read "Save (F9)" only for a brand-new, unsaved job and "Update (F9)" once the job has a saved record (`Model is not null`) — determined server-side from the same `Model is null` check the rest of the page already uses, so it flips automatically the moment a new job's first save redirects to its edit URL.

## Multi-tenant installation & licensing

The app is multi-client: every licensed company gets its own SQL Server database (all the entities described above — Party, CtdJob, JobIsne, Identity users, ...), but every installation is tracked through one central, always-reachable **ADMIN_CTD** database. There is no single fixed tenant connection string — the app resolves which database to talk to on every login, from the license number the user types in.

### ADMIN_CTD

A separate `AdminDbContext` (`Data/AdminDbContext.cs`), migrated independently from the tenant schema (`Data/Migrations/Admin/`, script at `database/scripts/02_AdminInitialCreate.sql`), with five tables:

| Table | Purpose |
|---|---|
| `Company` | One row per licensed client — name, code, address, GST, contact, status. |
| `License` | One row per issued license — `LicenseNumber` (`ERC00001` style), type (Trial/Standard/Professional/Enterprise), issue/expiry dates, machine identifier, an RSA signature (`LicenseKey`) and an AES-encrypted copy of the signed payload (`EncryptedLicense`). |
| `ClientDatabase` | The connection details for a company's own database — server, database name, username, and the password + full connection string, both AES-encrypted. Never stored in plain text. |
| `InstallationHistory` | One row per provisioning *attempt* (not per company) — a failed install followed by a retry leaves both in the audit trail. |
| `AdminNumberSequence` | Backs sequential License Number generation (`ERC00001`, `ERC00002`, ...). |

`AppDbContext.OnModelCreating` and `AdminDbContext.OnModelCreating` deliberately don't share configuration discovery: `AdminDbContext` applies its 5 `IEntityTypeConfiguration<T>` classes explicitly, and `AppDbContext`'s `ApplyConfigurationsFromAssembly` call filters out anything configuring a `CTD_FINAL.Entities.Admin` type — otherwise both `ApplyConfigurationsFromAssembly` calls would happily discover each other's configurations (they live in the same assembly) and leak Company/License/... into every tenant database, or Party/CtdJob/... into ADMIN_CTD.

### Installing a new client

Visit **`/Install`** — a 4-step wizard (Client Information → Database Configuration + a default Administrator account → Review → License Issued) that posts once to `InstallController.Provision`. That drives `ProvisioningService` (`Services/ProvisioningService.cs`) through the whole pipeline server-side:

1. `CREATE DATABASE`, `CREATE LOGIN`, `CREATE USER`, `ALTER ROLE db_owner ADD MEMBER` — every identifier is validated against an allow-list regex up front and only ever reaches SQL via `QUOTENAME()` inside parameterized dynamic SQL (`sp_executesql`), never raw string interpolation.
2. Deploys the full tenant schema by running `database/scripts/01_InitialCreate.sql` against the new database (`Infrastructure/Provisioning/SqlScriptRunner.cs` splits it into `GO`-separated batches).
3. Seeds the new database via `Data/Seed/TenantSeeder.cs` — Roles, the default Permission Matrix, and **exactly one** Administrator account (whatever email/name/password was entered in the wizard). No demo master data, no extra users — a fresh tenant starts genuinely empty except for what the app itself needs to function.
4. Generates a license (`ILicenseService.GenerateLicenseAsync`) and registers the `Company`/`License`/`ClientDatabase` rows in ADMIN_CTD, encrypting the database password and the runtime connection string.

Re-running the wizard once any company already exists requires the shared `Setup:InstallKey` (as a `?key=` query param), checked with a fixed-time comparison — there's no separate admin-auth system gating repeat installs.

**Local development**: if `ASPNETCORE_ENVIRONMENT=Development` and ADMIN_CTD has zero companies at startup, `Program.cs` attaches `ConnectionStrings:DefaultConnection`'s database directly as the first tenant — it doesn't go through `ProvisioningService`'s `CREATE DATABASE`/`LOGIN`/`USER` steps, since that database is expected to already exist and be migrated (`dotnet ef database update --context AppDbContext`), and connects to it the same way `DefaultConnection` always has (a trusted connection, not a freshly-created SQL login). It seeds it (`TenantSeeder`, idempotent) and registers it in ADMIN_CTD under the first generated license number — **`ERC00001`**, since `AdminNumberSequence` starts empty. Confirmed in the startup log: `Development bootstrap registered ... as the first tenant under license ERC00001 ...`. The seeded login is `admin@ctdsuite.local` / `ChangeMe#2026`. Any *later* client still goes through the real Install Wizard, which does create a brand-new database/login from scratch.

### Login flow

`Views/Account/Login.cshtml` collects a License Number alongside username/password. `AccountController.Login` then:

1. Validates the license (`ILicenseService.ValidateAsync`) — status, expiry, and an RSA signature check against the stored `LicenseKey` (skipped only if a license predates signing). A machine-identifier mismatch is logged, not blocked — a web-hosted app's "machine" is the deployment server, and legitimate hardware migrations shouldn't lock a paying customer out.
2. Resolves and decrypts the tenant's connection string (`ITenantResolutionService.ResolveAsync`, backed by `AdminDbContext`, cached 5 minutes) and stores it on `ITenantContextAccessor` — a scoped service the request's `AppDbContext` registration reads to build its connection (`Program.cs`'s `AddDbContext<AppDbContext>((sp, options) => ...)` resolves `ITenantContextAccessor` from the same DI scope the `DbContext` belongs to, so no existing service/repository/controller that depends on `AppDbContext` needed to change). `UserManager`/`SignInManager` are resolved lazily in `AccountController` (via `HttpContext.RequestServices`, not constructor injection) specifically so *loading* the login page never needs a tenant that hasn't been established yet.
3. Authenticates the user from *that* database via the existing `UserManager`/`SignInManager` — unchanged PBKDF2 password hashing, never reversible encryption.
4. On success, adds a `LicenseNumber` claim to the auth cookie (`Infrastructure/Identity/AppUserClaimsPrincipalFactory.cs`) so `Infrastructure/Identity/TenantCookieValidator.cs` — wired as the auth cookie's `OnValidatePrincipal` event — can re-resolve the same tenant on every later request in the session, re-checking status/expiry each time (cheap — no RSA verification), so a license suspended mid-session signs the user out on their next request instead of waiting for cookie expiry. This runs *inside* `OnValidatePrincipal` (before `UseAuthorization()`, even before MVC) rather than as separate middleware deliberately: ASP.NET Core Identity's own `OnValidatePrincipal` default (`SecurityStampValidator`) fires on every request carrying a cookie, and merely constructing it needs `AppDbContext` — so tenant resolution has to happen at that same point, ahead of it, or every authenticated request (not just login) hits the same "no tenant established" failure.

### Encryption

`Services/EncryptionService.cs` — AES-256-GCM (authenticated: confidentiality + tamper detection, no separate HMAC) for the database password, connection string, and license payload stored in ADMIN_CTD. `Services/LicenseService.cs` additionally signs the license payload with RSA-SHA256 (`Encryption:RsaPrivateKeyPem`/`RsaPublicKeyPem`) so direct database tampering with a `License` row is detectable at validation time, independent of the AES encryption.

`appsettings.json` ships a real (dev-only) AES key and RSA keypair so `dotnet run` works out of the box locally. **Generate your own before deploying anywhere real users can reach**:

```bash
openssl rand -base64 32                                    # Encryption:AesKeyBase64
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out private.pem
openssl rsa -in private.pem -pubout -out public.pem         # Encryption:RsaPrivateKeyPem / RsaPublicKeyPem
```

`appsettings.Production.json`/`appsettings.Testing.json` ship placeholders for all of these (`Encryption:*`, `ConnectionStrings:AdminConnection`/`ProvisioningConnection`, `Setup:InstallKey`) — override via environment variables or a secrets manager, same as `DefaultConnection`.

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
- SQL Server 2008 SP1+ (local instance, LocalDB, or `mcr.microsoft.com/mssql/server:2022-latest` via Docker)

### SQL Server 2008 compatibility

This app targets old SQL Server installations, down to **SQL Server 2008 SP1**. That constrains what EF Core is allowed to generate — most notably, `Skip()`/`Take()` cannot translate to SQL Server's `OFFSET ... FETCH NEXT` syntax, since that was only added in SQL Server 2012. Every paginated list (`JobIsneService`, `JobService`, `DocumentService`, `AuditService`) uses `Helpers/PagingExtensions.cs`'s `ToPagedResultAsync()` instead, which pulls the full filtered/sorted result set into the app and pages it in memory. That's fine at this app's scale, but means it isn't the right pattern to copy for a table expected to grow into the millions of rows — a raw-SQL `ROW_NUMBER() OVER (...)` query would be the 2008-compatible way to keep pagination server-side, if that's ever needed.

Everything else in the schema/queries (filtered unique indexes, `datetime2`, `GROUP BY`, `CROSS/OUTER APPLY` from `Include()`) has been in SQL Server since 2008 or earlier, so no other changes were needed for this — but this hasn't been exercised against a real SQL Server 2008 instance, only audited by inspecting the generated SQL and cross-checking against known SQL Server version history. If you hit another `Incorrect syntax near ...` error, it's almost certainly another EF Core LINQ translation using a newer T-SQL feature — check the exact SQL Server version the syntax was introduced in and report it back with the error text and the query that triggered it.

**One deliberate, narrower exception**: `ProvisioningService`'s tenant database setup uses `ALTER ROLE db_owner ADD MEMBER ...` to grant the newly-created login access to its new database — that syntax needs **SQL Server 2012+** (2008-compatible installs would need `sp_addrolemember` instead). This applies only to provisioning a *new* tenant database server, not to the app's day-to-day queries against an *existing* one — an existing tenant database still only needs 2008 SP1+ once it's been created.

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

3. **ADMIN_CTD gets migrated automatically on startup** (`AdminDbInitializer.SeedAsync` — migrate-only, it starts genuinely empty). There's no longer a single tenant database seeded alongside it — each client's own database is created and seeded separately, either through the Install Wizard or (in Development only) the auto-provision fallback described below. To migrate ADMIN_CTD manually instead:
   ```bash
   sqlcmd -S <server> -d ADMIN_CTD -i database/scripts/02_AdminInitialCreate.sql
   # or
   dotnet tool install -g dotnet-ef
   dotnet ef database update --context AdminDbContext
   ```
   A tenant database is never migrated this way — `01_InitialCreate.sql` / `dotnet ef database update --context AppDbContext` only apply against a database that's already been provisioned (created, login/user/role set up) by `ProvisioningService`, either via the wizard or manually.

4. **Run**:
   ```bash
   dotnet run --project CTD_FINAL.csproj
   ```
   In Development, if ADMIN_CTD has no companies yet, the app provisions a default tenant automatically on this first run — see "Multi-tenant installation & licensing" above for the seeded login and where to find its license number. Outside Development, visit `/Install` to provision the first client.

## Adding a schema migration

There are two independent `DbContext`s now — `AppDbContext` (tenant schema) and `AdminDbContext` (ADMIN_CTD) — every `dotnet ef` command needs `--context` to say which one:

```bash
dotnet ef migrations add <Name> --context AppDbContext
dotnet ef migrations script --context AppDbContext -o database/scripts/01_InitialCreate.sql --idempotent

dotnet ef migrations add <Name> --context AdminDbContext
dotnet ef migrations script --context AdminDbContext -o database/scripts/02_AdminInitialCreate.sql --idempotent
```

Both generated scripts need `SET QUOTED_IDENTIFIER ON; GO` re-added as the first lines by hand — `dotnet ef migrations script` doesn't emit it, but the filtered unique indexes in this schema require it when run via `sqlcmd` (`Microsoft.Data.SqlClient` already defaults it on, so `dotnet ef database update` is unaffected). It gets wiped every time the script is regenerated, so re-add it as part of the same change.

`AppDbContext.OnModelCreating`'s `ApplyConfigurationsFromAssembly` filter (see "Multi-tenant installation & licensing" above) means a new `IEntityTypeConfiguration<T>` for a `CTD_FINAL.Entities.Admin` type is automatically excluded from tenant migrations — but a new tenant-side entity's configuration must still live outside that namespace, or it'll be silently excluded too.

## Security notes

- Every mutating (`POST`) action validates the anti-forgery token; AJAX JSON calls supply it via the `RequestVerificationToken` header — including the anonymous `/Install/Provision` endpoint, which still requires a valid token even though it doesn't require a signed-in user.
- All authorization is enforced server-side via `[RequirePermission]`, independent of the (also permission-gated) navigation UI.
- Tenant data access goes through EF Core's parameterized LINQ — no raw/interpolated SQL. The one deliberate exception is `ProvisioningService`'s `CREATE DATABASE`/`CREATE LOGIN`/`CREATE USER`/`ALTER ROLE` statements (EF Core has no API for these), which are raw ADO.NET but never string-interpolated: every identifier is validated against an allow-list regex first and only reaches SQL via `QUOTENAME()` inside parameterized dynamic SQL.
- Database passwords, connection strings and license payloads are AES-256-GCM encrypted at rest in ADMIN_CTD (`Services/EncryptionService.cs`); license payloads are additionally RSA-SHA256 signed (`Services/LicenseService.cs`) so direct database tampering is detectable. User login passwords are untouched by any of this — they stay on ASP.NET Core Identity's one-way PBKDF2 hashing.
- Uploaded documents are stored under `App_Data/uploads/documents`, outside `wwwroot`, so the static file middleware can never serve them directly; every download is re-authorized through `Documents/Download`. Uploads are restricted to a fixed extension allow-list and a 20 MB size cap.
- Identity passwords require 8+ characters with a digit, uppercase letter and non-alphanumeric character; accounts lock out for 10 minutes after 5 failed attempts.
- Cookies are `HttpOnly`, `SameSite=Lax`; `UseHttpsRedirection`/`UseHsts` are enabled; baseline security response headers (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`) are set on every response.
