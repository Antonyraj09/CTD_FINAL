using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

/// <summary>Singleton row backing the Settings screen's numbering + company profile panels.</summary>
public class AppSettingsEntity : BaseEntity
{
    [Required, StringLength(10)]
    public string JobNumberPrefix { get; set; } = "CTD";

    [Required, StringLength(10)]
    public string InvoicePrefix { get; set; } = "INV";

    [Required, StringLength(10)]
    public string DocumentPrefix { get; set; } = "DOC";

    [Required, StringLength(200)]
    public string CompanyName { get; set; } = "Himalayan Cargo Movers Pvt. Ltd.";

    [Required, StringLength(400)]
    public string CompanyAddress { get; set; } = "Plot 14, ICD Birgunj Road, Raxaul, Bihar, India";

    [Required, StringLength(20)]
    public string CompanyGstin { get; set; } = "10AABCH1234D1Z5";

    [Required, StringLength(30)]
    public string ChaLicenseNo { get; set; } = "CHA/PAT/0042";
}
