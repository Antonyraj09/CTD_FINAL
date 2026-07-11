using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

/// <summary>Backs the Role Permission Matrix on the Users &amp; Roles screen.</summary>
public class RolePermission : BaseEntity
{
    [Required, StringLength(50)]
    public string Role { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string ModuleKey { get; set; } = string.Empty;

    public bool Allowed { get; set; }
}
