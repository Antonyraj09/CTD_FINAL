using CTD_FINAL.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

/// <summary>
/// Backs every prototype nav entry that has no functioning screen behind it yet.
/// Two kinds of entries share this controller, exactly as the prototype's own
/// "Under Development" badge sections did for its ~24 future-ERP-module stubs:
///   1. Genuine prototype placeholders (EDI Import/Export, Job MISCN/MISCT, Delivery
///      variants, Accounts vouchers, Report 2/3) — permanent, title/description
///      copied verbatim from the prototype.
///   2. Real screens not yet wired up in the current build phase — temporary,
///      replaced by a real controller/action as each phase lands.
/// </summary>
public class PlaceholderController : Controller
{
    private static readonly Dictionary<string, PlaceholderViewModel> Registry = new(StringComparer.OrdinalIgnoreCase)
    {
        // ---- Genuine prototype "Under Development" screens ----
        ["edi-import-iae"] = New("EDI Import (IAE)", "Air EDI Import — Import Advice / Entry"),
        ["edi-import-ise"] = New("EDI Import (ISE)", "Sea EDI Import — Import Sea Entry"),
        ["edi-export-eae"] = New("EDI Export (EAE)", "Air EDI Export — Export Advice / Entry"),
        ["edi-export-ese"] = New("EDI Export (ESE)", "Sea EDI Export — Export Sea Entry"),
        ["job-esne-esbe"] = New("Job ESNE / ESBE", "Export Sea / Nepal Entry & Export Sea Bill of Entry"),
        ["job-miscn"] = New("Job MISCN", "Miscellaneous Nepal Cargo Job Entry"),
        ["job-misct"] = New("Job MISCT", "Miscellaneous Cargo Transit Job Entry"),
        ["job-isne-browse"] = New("Job ISNE Browse", "Browse and search all ISNE jobs"),
        ["edi-job-register"] = New("EDI Job Register", "EDI Job Register — IAE / ISE / EAE view"),
        ["delivery-isne"] = New("Delivery ISNE", "Process delivery for ISNE jobs"),
        ["delivery-challan"] = New("Delivery Challan", "Generate delivery challan documents"),
        ["delivery-export"] = New("Delivery Export", "Manage export delivery processing"),
        ["delivery-misct"] = New("Delivery MISCT", "Delivery for miscellaneous transit cargo"),
        ["exchange-rate"] = New("Exchange Rate", "Manage foreign exchange rates"),
        ["slot-date"] = New("Slot Date", "Manage slot booking dates"),
        ["billing"] = New("Billing", "Process job billing and invoices"),
        ["billing-browse"] = New("Billing Browse", "Browse and search billing records"),
        ["petty-cash"] = New("Petty Cash Voucher", "Record petty cash transactions"),
        ["cash-bank-voucher"] = New("Cash / Bank Voucher", "Manage cash and bank payment vouchers"),
        ["journal-voucher"] = New("Journal Voucher", "Post journal entries and adjustments"),
        ["purchase-bill"] = New("Purchase Bill", "Record and manage purchase bills"),
        ["purchase-tds"] = New("Purchase TDS Booked", "Manage TDS deductions on purchases"),
        ["purchase-payment"] = New("Purchase Payment", "Process and record purchase payments"),
        ["report-2"] = New("Report 2", "Custom report — configuration pending"),
        ["report-3"] = New("Report 3", "Custom report — configuration pending"),
    };

    private static PlaceholderViewModel New(string title, string description) =>
        new() { Title = title, Description = description };

    [HttpGet("/Placeholder/{slug}")]
    public IActionResult Index(string slug)
    {
        var model = Registry.TryGetValue(slug, out var found)
            ? found
            : new PlaceholderViewModel { Title = slug, Description = "This screen is scheduled for a future build phase.", ComingInFutureBuildPhase = true };

        return View(model);
    }
}
