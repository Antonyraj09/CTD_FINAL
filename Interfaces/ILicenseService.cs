using CTD_FINAL.Entities.Admin;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Interfaces;

public record LicenseValidationResult(bool IsValid, string? FailureReason, License? License);

/// <summary>
/// Issues and validates licenses. Licensing is deliberately split from tenant/connection
/// resolution (ITenantResolutionService): this service does the heavier, cryptographic
/// checks (RSA signature verification, machine-identifier binding) once at login time;
/// TenantResolutionService does the cheap status/expiry re-check on every subsequent
/// request so normal page views don't pay for a signature verification each time.
/// </summary>
public interface ILicenseService
{
    /// <summary>Builds a new, unsaved License (next ERCxxxxx number, signed + encrypted payload) for the caller to persist alongside the Company/ClientDatabase rows it belongs with.</summary>
    Task<License> GenerateLicenseAsync(int companyId, string companyCode, LicenseType licenseType, string applicationVersion, CancellationToken ct = default);

    Task<LicenseValidationResult> ValidateAsync(string licenseNumber, string machineIdentifier, CancellationToken ct = default);

    /// <summary>Recomputes LicenseKey/EncryptedLicense from a License's current field values
    /// (LicenseNumber, LicenseType, IssueDate, ExpiryDate, ApplicationVersion) plus its
    /// company's code. Call this whenever an already-issued license's signed fields are
    /// edited (e.g. LicenseType or ExpiryDate from an admin screen) — without it, the stored
    /// signature would no longer match what ValidateAsync recomputes and the client would be
    /// rejected as tampered at next login, even though nothing malicious happened.</summary>
    void ReissueSignature(License license, string companyCode);

    /// <summary>Marks LastValidation always, and on first-ever success also Activated + MachineIdentifier.</summary>
    Task RecordSuccessfulActivationAsync(License license, string machineIdentifier, CancellationToken ct = default);

    string ComputeCurrentMachineIdentifier();
}
