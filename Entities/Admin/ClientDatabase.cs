using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities.Admin;

/// <summary>
/// The connection details for a company's own SQL Server database. This is the row
/// the login flow reads (by CompanyId, resolved via the License Number) to build a
/// dynamic connection string at request time — the app never has a fixed one.
/// </summary>
public class ClientDatabase : BaseEntity
{
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    [Required, StringLength(128)]
    public string DatabaseName { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string ServerName { get; set; } = string.Empty;

    [Required, StringLength(128)]
    public string DatabaseUsername { get; set; } = string.Empty;

    /// <summary>AES-encrypted (base64) SQL login password — never stored in plain text.</summary>
    [Required, StringLength(500)]
    public string EncryptedPassword { get; set; } = string.Empty;

    /// <summary>AES-encrypted (base64) full ADO.NET connection string — what login actually decrypts and connects with.</summary>
    [Required, StringLength(1000)]
    public string EncryptedConnectionString { get; set; } = string.Empty;

    [StringLength(50)]
    public string? DatabaseVersion { get; set; }

    [StringLength(20)]
    public string? ApplicationVersion { get; set; }

    public ClientDatabaseStatus Status { get; set; } = ClientDatabaseStatus.Provisioning;
}
