namespace CTD_FINAL.Models.JobIsne;

public class JobIsneSaveRequest
{
    public int Id { get; set; }

    // Section A
    public DateTime JobDate { get; set; }
    public string PartyCode { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? SubAgentCode { get; set; }
    public string? SubAgentName { get; set; }
    public string? CtdNumber { get; set; }
    public DateTime? CtdDate { get; set; }
    public string? VesselName { get; set; }
    public string? VoyageNo { get; set; }
    public string? TsVessel { get; set; }
    public string? TsVoyage { get; set; }
    public string? CountryCgn { get; set; }
    public string? CountryOrigin { get; set; }
    public string? RouteOfTransit { get; set; }
    public string? RotNo { get; set; }
    public DateTime? RotDate { get; set; }
    public DateTime? InwardDate { get; set; }
    public string? LineNo { get; set; }

    // Section B
    public string? MblNo { get; set; }
    public DateTime? MblDate { get; set; }
    public string? HblNo { get; set; }
    public DateTime? HblDate { get; set; }
    public string? IlNo { get; set; }
    public DateTime? IlDate { get; set; }
    public string? LcNo { get; set; }
    public DateTime? LcDate { get; set; }
    public string? AccountName { get; set; }
    public string? BankName { get; set; }
    public string? RefNo { get; set; }
    public DateTime? RefDate { get; set; }
    public string? SteamerAgent { get; set; }
    public string? ContainerAgent { get; set; }

    // Section C
    public DateTime? VesselArrival { get; set; }
    public string? CtdSentTo { get; set; }
    public bool GreenCtd { get; set; }
    public DateTime? DuePackingList { get; set; }
    public DateTime? DueInvoice { get; set; }
    public DateTime? DueOriginalBl { get; set; }
    public DateTime? DueInsuranceCert { get; set; }
    public DateTime? DueLcCopy { get; set; }
    public DateTime? DueLoa { get; set; }
    public DateTime? DueOrigin { get; set; }
    public DateTime? DueProformaInvoice { get; set; }

    // Section D
    public string ShipmentType { get; set; } = "FCL";
    public string? MiscDescription { get; set; }
    public string? CargoDescription { get; set; }
    public List<JobIsneContainerRequest> Containers { get; set; } = new();

    // Section E: Entry for Data Sheet (all optional)
    public string? ImporterCode { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? CertificateOfOrigin { get; set; }
    public DateTime? CertificateOfOriginDate { get; set; }
    public bool SensitiveCargo { get; set; }
    public string? InsuranceCompanyNameAddress { get; set; }
    public decimal? SensitiveCifValue { get; set; }

    // Section F
    public string Currency { get; set; } = "USD";
    public decimal? ExchangeRate { get; set; }
    public decimal? FobValue { get; set; }
    public decimal? Freight { get; set; }
    public decimal? CifFc { get; set; }
    public decimal? CifFcReference { get; set; }
    public decimal? InsuranceFc { get; set; }
    public decimal? InsuranceValue { get; set; }
    public decimal? InsuranceExRate { get; set; }
    public decimal? InsuranceRate { get; set; }
    public decimal? InsuranceValueInr { get; set; }
    public decimal? CifInr { get; set; }
    public decimal? MarketRate { get; set; }
    public decimal? MarketValueInr { get; set; }
    public decimal? GrossWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public decimal? LcAmount { get; set; }
    public DateTime? ShipmentExpiry { get; set; }
    public string PartialShipment { get; set; } = "ALLOWED";
    public decimal? DutyAmount { get; set; }
}

public class JobIsneContainerRequest
{
    public string? ContainerNo { get; set; }
    public string ContainerSize { get; set; } = "20ft";
    public string ShipmentType { get; set; } = "FCL";
    public int NoPackages { get; set; }
    public string? PackageType { get; set; }
    public decimal? GrossWeight { get; set; }
    public string GrossWeightUnit { get; set; } = "KG";
    public decimal? NetWeight { get; set; }
    public string NetWeightUnit { get; set; } = "KG";
    public string? MarksSerial { get; set; }
    public string? CustomsCode { get; set; }
}
