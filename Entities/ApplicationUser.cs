using Microsoft.AspNetCore.Identity;

namespace CTD_FINAL.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    /// <summary>Free-text association to an Importer for Viewer-role (customer) users, used by the Customer Dashboard.</summary>
    public int? ImporterId { get; set; }
}

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
