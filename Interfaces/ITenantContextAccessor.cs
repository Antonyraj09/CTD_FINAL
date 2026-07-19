namespace CTD_FINAL.Interfaces;

/// <summary>
/// Scoped, request-lifetime holder for "which tenant is this request talking to."
/// Set once per request — either by AccountController.Login (before UserManager/
/// SignInManager touch AppDbContext for the very first time) or by
/// TenantResolutionMiddleware on every subsequent authenticated request (reading the
/// LicenseNumber claim embedded on the auth cookie) — then read by AppDbContext's DI
/// registration to build a connection dynamically. Nothing else in the app should ever
/// need a different mechanism for "what tenant am I" — this is the single source of truth.
/// </summary>
public interface ITenantContextAccessor
{
    string? ConnectionString { get; }
    string? LicenseNumber { get; }
    int? CompanyId { get; }
    string? CompanyName { get; }

    void Set(string connectionString, string licenseNumber, int companyId, string companyName);
}
