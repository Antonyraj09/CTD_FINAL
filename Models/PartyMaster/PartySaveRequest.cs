namespace CTD_FINAL.Models.PartyMaster;

public class PartySaveRequest
{
    public int Id { get; set; }

    public string PartyCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string Constitution { get; set; } = "PrivateLimited";
    public string? Pan { get; set; }
    public string? IecCode { get; set; }
    public string? CinNumber { get; set; }

    public bool IsImporter { get; set; }
    public bool IsTransporter { get; set; }
    public bool IsAgent { get; set; }

    public string? License { get; set; }
    public DateTime? LicenseValidUpto { get; set; }
    public string? Fleet { get; set; }
    public string? SubAgentCode { get; set; }

    public string AeoStatus { get; set; } = "None";
    public string? AeoCertificateNo { get; set; }

    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankIfsc { get; set; }
    public string? AdCode { get; set; }

    public string? Website { get; set; }
    public string? ContactPersonName { get; set; }
    public string? ContactPersonDesignation { get; set; }
    public string? ContactPersonPhone { get; set; }
    public string? ContactPersonEmail { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }

    public List<PartyBranchRequest> Branches { get; set; } = new();
}

public class PartyBranchRequest
{
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
