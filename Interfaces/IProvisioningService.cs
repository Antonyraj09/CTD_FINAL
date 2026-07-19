using CTD_FINAL.Enums;

namespace CTD_FINAL.Interfaces;

/// <summary>Everything Install Wizard Steps 1-2 collect, handed to ProvisioningService to run Step 3 (create the client database/login/user/role, deploy schema, seed) and Step 4 (register the installation in ADMIN_CTD) in one call.</summary>
public record ProvisioningRequest(
    string CompanyName,
    string CompanyCode,
    string? Address,
    string? Country,
    string? State,
    string? City,
    string? GstNumber,
    string? ContactPerson,
    string Email,
    string? Phone,
    string? InstallationLocation,
    LicenseType LicenseType,
    string DatabaseName,
    string DatabaseUsername,
    string DatabasePassword,
    string AdminEmail,
    string AdminFullName,
    string AdminPassword,
    string InstalledBy,
    string MachineName);

public record ProvisioningResult(bool Success, string? LicenseNumber, string? CompanyCode, string? FailureReason);

/// <summary>
/// Runs Installation Wizard Step 3 (CREATE DATABASE/LOGIN/USER, db_owner role membership,
/// full application schema deployment, TenantSeeder) and Step 4 (register the resulting
/// Company/License/ClientDatabase rows in ADMIN_CTD, encrypting the database password and
/// connection string). Never inserts business data — only what TenantSeeder seeds.
/// </summary>
public interface IProvisioningService
{
    Task<ProvisioningResult> ProvisionAsync(ProvisioningRequest request, CancellationToken ct = default);
}
