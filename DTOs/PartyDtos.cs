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
    public string Country { get; set; } = "India";
    public string? Gstin { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPersonName { get; set; }
    public string? CustomsRegistrationNo { get; set; }
}
