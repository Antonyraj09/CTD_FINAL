namespace CTD_FINAL.DTOs;

public class PartyBranchDto
{
    public int Id { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string Country { get; set; } = "Nepal";
    public string? Gstin { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPersonName { get; set; }
    public string? CustomsRegistrationNo { get; set; }
}

/// <summary>Slim projection for the Party Master list — only the columns the table row
/// actually renders, and only the primary-branch fields (not the full branch/registration
/// detail) instead of the whole entity graph the Edit screen needs.</summary>
public class PartyListItem
{
    public int Id { get; set; }
    public string? PartyCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public bool IsImporter { get; set; }
    public bool IsTransporter { get; set; }
    public bool IsAgent { get; set; }
    public string? Pan { get; set; }
    public string? IecCode { get; set; }
    public bool IsActive { get; set; }
    public List<PartyListBranchItem> Branches { get; set; } = new();

    public string Roles => string.Join(", ", new[]
    {
        IsImporter ? "Importer" : null,
        IsTransporter ? "Transporter" : null,
        IsAgent ? "Agent" : null
    }.Where(r => r != null));
}

public class PartyListBranchItem
{
    public bool IsPrimary { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Gstin { get; set; }
}
