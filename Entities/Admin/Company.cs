using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities.Admin;

/// <summary>
/// A licensed client company, one row per tenant. Lives in the ADMIN_CTD database
/// (AdminDbContext), never in a tenant's own database — this is the record that lets
/// the login flow resolve "which client database does this License Number map to."
/// </summary>
public class Company : BaseEntity
{
    [Required, StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>Short unique code (e.g. "ABCLOG") used to derive default database/user names.</summary>
    [Required, StringLength(20)]
    public string CompanyCode { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(20)]
    public string? GstNumber { get; set; }

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [Required, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    /// <summary>Free-text description of where the app is installed (server name, region, on-prem site, etc.).</summary>
    [StringLength(200)]
    public string? InstallationLocation { get; set; }

    public CompanyStatus Status { get; set; } = CompanyStatus.Active;

    public ICollection<License> Licenses { get; set; } = new List<License>();
    public ICollection<ClientDatabase> ClientDatabases { get; set; } = new List<ClientDatabase>();
}
