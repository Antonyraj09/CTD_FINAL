namespace CTD_FINAL.Enums;

public enum LicenseType
{
    Trial = 0,
    Standard = 1,
    Professional = 2,
    Enterprise = 3
}

public enum LicenseStatus
{
    Active = 0,
    Suspended = 1,
    Expired = 2,
    Revoked = 3
}

public enum CompanyStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2
}

public enum ClientDatabaseStatus
{
    Provisioning = 0,
    Active = 1,
    Failed = 2,
    Suspended = 3
}

public enum InstallationStatus
{
    Started = 0,
    Succeeded = 1,
    Failed = 2
}
