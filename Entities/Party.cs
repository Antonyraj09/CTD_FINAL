using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

/// <summary>
/// Unified business-partner master, replacing the three single-role
/// Importer/Agent/Transporter masters. One company can now be tagged with
/// any combination of roles via IsImporter/IsTransporter/IsAgent instead of
/// needing a separate record (and separate identity) in three different
/// tables for what is often the same real-world entity.
/// </summary>
public class Party : BaseEntity
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    public bool IsImporter { get; set; }
    public bool IsTransporter { get; set; }
    public bool IsAgent { get; set; }

    /// <summary>Importer-specific.</summary>
    [StringLength(20)]
    public string? Gstin { get; set; }

    /// <summary>Agent (CHA)-specific.</summary>
    [StringLength(50)]
    public string? License { get; set; }

    /// <summary>Transporter-specific.</summary>
    [StringLength(100)]
    public string? Fleet { get; set; }

    public string Roles => string.Join(", ", new[]
    {
        IsImporter ? "Importer" : null,
        IsTransporter ? "Transporter" : null,
        IsAgent ? "Agent" : null
    }.Where(r => r != null));
}
