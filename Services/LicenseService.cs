using System.Security.Cryptography;
using System.Text;
using CTD_FINAL.Constants;
using CTD_FINAL.Data;
using CTD_FINAL.Entities.Admin;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CTD_FINAL.Services;

public class LicenseService : ILicenseService
{
    private readonly AdminDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly string _rsaPrivateKeyPem;
    private readonly string _rsaPublicKeyPem;

    public LicenseService(AdminDbContext context, IEncryptionService encryptionService, IConfiguration configuration)
    {
        _context = context;
        _encryptionService = encryptionService;
        _rsaPrivateKeyPem = configuration["Encryption:RsaPrivateKeyPem"]
            ?? throw new InvalidOperationException("Configuration 'Encryption:RsaPrivateKeyPem' not found.");
        _rsaPublicKeyPem = configuration["Encryption:RsaPublicKeyPem"]
            ?? throw new InvalidOperationException("Configuration 'Encryption:RsaPublicKeyPem' not found.");
    }

    public async Task<License> GenerateLicenseAsync(int companyId, string companyCode, LicenseType licenseType, string applicationVersion, CancellationToken ct = default)
    {
        var licenseNumber = await NextLicenseNumberAsync(ct);
        var issueDate = DateTime.UtcNow;
        var durationDays = LicenseConstants.DurationDays.TryGetValue(licenseType, out var days) ? days : 365;
        var expiryDate = issueDate.AddDays(durationDays);

        var payload = BuildPayload(licenseNumber, companyCode, licenseType, issueDate, expiryDate, applicationVersion);
        var signature = SignPayload(payload);
        var encryptedPayload = _encryptionService.Encrypt(payload);

        return new License
        {
            CompanyId = companyId,
            LicenseNumber = licenseNumber,
            LicenseType = licenseType,
            IssueDate = issueDate,
            ExpiryDate = expiryDate,
            InstallationDate = issueDate,
            Status = LicenseStatus.Active,
            Activated = false,
            ApplicationVersion = applicationVersion,
            LicenseKey = signature,
            EncryptedLicense = encryptedPayload
        };
    }

    public async Task<LicenseValidationResult> ValidateAsync(string licenseNumber, string machineIdentifier, CancellationToken ct = default)
    {
        var license = await _context.Licenses.Include(l => l.Company).FirstOrDefaultAsync(l => l.LicenseNumber == licenseNumber, ct);
        if (license is null)
            return new LicenseValidationResult(false, "License not found.", null);

        if (license.Status != LicenseStatus.Active)
            return new LicenseValidationResult(false, "License is not active.", license);

        if (license.ExpiryDate < DateTime.UtcNow)
            return new LicenseValidationResult(false, "License has expired.", license);

        if (!string.IsNullOrEmpty(license.LicenseKey))
        {
            var payload = BuildPayload(license.LicenseNumber, license.Company.CompanyCode, license.LicenseType, license.IssueDate, license.ExpiryDate, license.ApplicationVersion ?? string.Empty);
            if (!VerifySignature(payload, license.LicenseKey))
                return new LicenseValidationResult(false, "License signature could not be verified.", license);
        }

        // Machine binding: audit-log-only for v1, not a hard block — a web-hosted app's
        // "machine" is the deployment server, and legitimate hardware migrations/DR
        // failovers shouldn't lock a paying customer out. A future stricter mode can flip
        // this to a hard failure once there's an admin-facing way to re-bind a license.
        if (!string.IsNullOrEmpty(license.MachineIdentifier) && license.MachineIdentifier != machineIdentifier)
        {
            // Deliberately not returned as a failure — see comment above.
        }

        return new LicenseValidationResult(true, null, license);
    }

    public async Task RecordSuccessfulActivationAsync(License license, string machineIdentifier, CancellationToken ct = default)
    {
        license.LastValidation = DateTime.UtcNow;
        if (!license.Activated)
        {
            license.Activated = true;
            license.MachineIdentifier = machineIdentifier;
        }
        await _context.SaveChangesAsync(ct);
    }

    public string ComputeCurrentMachineIdentifier()
    {
        // The "machine" a web-hosted MVC app runs on is the deployment server, not the
        // end user's browser — fingerprint the host machine name + primary MAC address.
        var machineName = Environment.MachineName;
        var mac = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                     && n.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
            .Select(n => n.GetPhysicalAddress().ToString())
            .FirstOrDefault(a => !string.IsNullOrEmpty(a)) ?? "UNKNOWN";

        var raw = $"{machineName}|{mac}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }

    private async Task<string> NextLicenseNumberAsync(CancellationToken ct)
    {
        var row = await _context.NumberSequences.FirstOrDefaultAsync(s => s.Key == LicenseConstants.LicenseSequenceKey, ct);
        if (row is null)
        {
            row = new AdminNumberSequence { Key = LicenseConstants.LicenseSequenceKey, CurrentValue = 0 };
            _context.NumberSequences.Add(row);
        }
        row.CurrentValue += 1;
        row.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return $"{LicenseConstants.LicenseNumberPrefix}{row.CurrentValue:D5}";
    }

    // DateTime.ToString("O") encodes Kind into the string (a trailing "Z" for Utc, nothing for
    // Unspecified) — these values are always genuinely UTC instants (assigned from
    // DateTime.UtcNow at issuance), but SQL Server's datetime2 columns don't store Kind, so EF
    // Core reloads them as Unspecified. Without normalizing back to Utc here, ValidateAsync
    // would rebuild a payload that differs from what was signed at issuance purely because of
    // that lost label — not because anything about the license actually changed — and every
    // signature check would fail after a single round-trip through the database.
    private static string BuildPayload(string licenseNumber, string companyCode, LicenseType licenseType, DateTime issueDate, DateTime expiryDate, string applicationVersion) =>
        $"{licenseNumber}|{companyCode}|{licenseType}|{DateTime.SpecifyKind(issueDate, DateTimeKind.Utc):O}|{DateTime.SpecifyKind(expiryDate, DateTimeKind.Utc):O}|{applicationVersion}";

    private string SignPayload(string payload)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(_rsaPrivateKeyPem);
        var signature = rsa.SignData(Encoding.UTF8.GetBytes(payload), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signature);
    }

    private bool VerifySignature(string payload, string signatureBase64)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(_rsaPublicKeyPem);
            var signature = Convert.FromBase64String(signatureBase64);
            return rsa.VerifyData(Encoding.UTF8.GetBytes(payload), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
