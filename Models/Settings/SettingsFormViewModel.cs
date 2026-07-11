using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Models.Settings;

public class SettingsFormViewModel
{
    [Required, StringLength(10)]
    [Display(Name = "Job Number Prefix")]
    public string JobNumberPrefix { get; set; } = "CTD";

    [Required, StringLength(10)]
    [Display(Name = "Invoice Prefix")]
    public string InvoicePrefix { get; set; } = "INV";

    [Required, StringLength(10)]
    [Display(Name = "Document Prefix")]
    public string DocumentPrefix { get; set; } = "DOC";

    [Required, StringLength(200)]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required, StringLength(400)]
    [Display(Name = "Address")]
    public string CompanyAddress { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "GSTIN")]
    public string CompanyGstin { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [Display(Name = "CHA License No.")]
    public string ChaLicenseNo { get; set; } = string.Empty;
}
