using CTD_FINAL.Data;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Models.ReportCenter;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class ReportCenterService : IReportCenterService
{
    private const int MaxHitsPerCategory = 15;

    private readonly AppDbContext _context;

    public ReportCenterService(AppDbContext context) => _context = context;

    private static string D(DateTime? d) => d?.ToString("dd/MM/yyyy") ?? "";

    public async Task<List<ReportCenterCategory>> SearchAsync(string term, ISet<string> allowedCategoryKeys, CancellationToken ct = default)
    {
        var results = new List<ReportCenterCategory>();
        if (string.IsNullOrWhiteSpace(term)) return results;
        term = term.Trim();

        if (allowedCategoryKeys.Contains("jobIsne"))
            results.Add(await SearchJobIsneAsync(term, ct));
        if (allowedCategoryKeys.Contains("deliveryIsne"))
            results.Add(await SearchDeliveryIsneAsync(term, ct));
        if (allowedCategoryKeys.Contains("ctdJob"))
            results.Add(await SearchCtdJobAsync(term, ct));
        if (allowedCategoryKeys.Contains("party"))
            results.Add(await SearchPartyAsync(term, ct));
        if (allowedCategoryKeys.Contains("subAgent"))
            results.Add(await SearchSubAgentAsync(term, ct));
        if (allowedCategoryKeys.Contains("transitRoute"))
            results.Add(await SearchTransitRouteAsync(term, ct));
        if (allowedCategoryKeys.Contains("document"))
            results.Add(await SearchDocumentAsync(term, ct));

        // Empty categories add noise without adding information — only surface ones that
        // actually matched something.
        return results.Where(c => c.Hits.Count > 0).ToList();
    }

    private async Task<ReportCenterCategory> SearchJobIsneAsync(string term, CancellationToken ct)
    {
        var rows = await _context.JobIsnes.AsNoTracking()
            .Where(j => j.JobNumber.Contains(term)
                || j.PartyName.Contains(term)
                || (j.ImporterCode != null && j.ImporterCode.Contains(term))
                || (j.InvoiceNumber != null && j.InvoiceNumber.Contains(term))
                || (j.CtdNumber != null && j.CtdNumber.Contains(term))
                || (j.RouteOfTransit != null && j.RouteOfTransit.Contains(term))
                || (j.MblNo != null && j.MblNo.Contains(term))
                || (j.HblNo != null && j.HblNo.Contains(term))
                || j.Containers.Any(c => (c.ContainerNo != null && c.ContainerNo.Contains(term)) || (c.SealNumbers != null && c.SealNumbers.Contains(term))))
            .OrderByDescending(j => j.JobDate)
            .Take(MaxHitsPerCategory)
            .Select(j => new ReportCenterHit
            {
                Title = j.JobNumber,
                Subtitle = j.PartyName,
                Date = D(j.JobDate),
                Url = $"/JobIsne/Index?id={j.Id}"
            })
            .ToListAsync(ct);

        return new ReportCenterCategory { Key = "jobIsne", Label = "Job ISNE", Hits = rows };
    }

    private async Task<ReportCenterCategory> SearchDeliveryIsneAsync(string term, CancellationToken ct)
    {
        var rows = await _context.DeliveryIsnes.AsNoTracking()
            .Where(d => d.DeletedAt == null && (
                d.JobNo.Contains(term)
                || (d.CustomerName != null && d.CustomerName.Contains(term))
                || (d.ConsigneeName != null && d.ConsigneeName.Contains(term))
                || (d.ContainerNo != null && d.ContainerNo.Contains(term))
                || (d.BslNo != null && d.BslNo.Contains(term))
                || (d.TruckRailwayReckNo != null && d.TruckRailwayReckNo.Contains(term))
                || (d.TransporterName != null && d.TransporterName.Contains(term))))
            .OrderByDescending(d => d.DeliveryDate)
            .Take(MaxHitsPerCategory)
            .Select(d => new ReportCenterHit
            {
                Title = d.JobNo,
                Subtitle = d.CustomerName ?? d.ConsigneeName ?? "",
                Date = D(d.DeliveryDate),
                Url = $"/DeliveryIsne/Entry?id={d.Id}"
            })
            .ToListAsync(ct);

        return new ReportCenterCategory { Key = "deliveryIsne", Label = "Delivery ISNE", Hits = rows };
    }

    private async Task<ReportCenterCategory> SearchCtdJobAsync(string term, CancellationToken ct)
    {
        var rows = await _context.CtdJobs.AsNoTracking()
            .Where(j => j.JobNo.Contains(term)
                || (j.InvoiceNo != null && j.InvoiceNo.Contains(term))
                || (j.CtdNumber != null && j.CtdNumber.Contains(term))
                || (j.HsCode != null && j.HsCode.Contains(term))
                || (j.Importer != null && j.Importer.Name.Contains(term)))
            .OrderByDescending(j => j.JobDate)
            .Take(MaxHitsPerCategory)
            .Select(j => new ReportCenterHit
            {
                Title = j.JobNo,
                Subtitle = j.Importer != null ? j.Importer.Name : "",
                Date = D(j.JobDate),
                Url = $"/Jobs/Wizard?id={j.Id}"
            })
            .ToListAsync(ct);

        return new ReportCenterCategory { Key = "ctdJob", Label = "CTD Job", Hits = rows };
    }

    private async Task<ReportCenterCategory> SearchPartyAsync(string term, CancellationToken ct)
    {
        var rows = await _context.Parties.AsNoTracking()
            .Where(p => (p.PartyCode != null && p.PartyCode.Contains(term))
                || p.Name.Contains(term)
                || (p.TradeName != null && p.TradeName.Contains(term))
                || (p.Pan != null && p.Pan.Contains(term))
                || (p.IecCode != null && p.IecCode.Contains(term)))
            .OrderBy(p => p.Name)
            .Take(MaxHitsPerCategory)
            .Select(p => new ReportCenterHit
            {
                Title = p.Name,
                Subtitle = p.PartyCode ?? "",
                Url = $"/Party/Edit?id={p.Id}"
            })
            .ToListAsync(ct);

        return new ReportCenterCategory { Key = "party", Label = "Party Master", Hits = rows };
    }

    private async Task<ReportCenterCategory> SearchSubAgentAsync(string term, CancellationToken ct)
    {
        var rows = await _context.SubAgents.AsNoTracking()
            .Where(a => a.Name.Contains(term)
                || a.SubAgentCode.Contains(term)
                || (a.LicenseNo != null && a.LicenseNo.Contains(term)))
            .OrderBy(a => a.Name)
            .Take(MaxHitsPerCategory)
            .Select(a => new ReportCenterHit
            {
                Title = a.Name,
                Subtitle = a.LicenseNo ?? "",
                Url = "/Masters/Index?tab=subagent"
            })
            .ToListAsync(ct);

        return new ReportCenterCategory { Key = "subAgent", Label = "Sub-Agent", Hits = rows };
    }

    private async Task<ReportCenterCategory> SearchTransitRouteAsync(string term, CancellationToken ct)
    {
        var rows = await _context.TransitRoutes.AsNoTracking()
            .Where(r => r.Name.Contains(term))
            .OrderBy(r => r.Name)
            .Take(MaxHitsPerCategory)
            .Select(r => new ReportCenterHit
            {
                Title = r.Name,
                Subtitle = r.Distance ?? "",
                Url = "/Masters/Index?tab=route"
            })
            .ToListAsync(ct);

        return new ReportCenterCategory { Key = "transitRoute", Label = "Transit Route", Hits = rows };
    }

    private async Task<ReportCenterCategory> SearchDocumentAsync(string term, CancellationToken ct)
    {
        var rows = await _context.GeneratedDocuments.AsNoTracking()
            .Where(d => d.Name.Contains(term)
                || (d.JobNo != null && d.JobNo.Contains(term))
                || d.Type.Contains(term))
            .OrderByDescending(d => d.DocumentDate)
            .Take(MaxHitsPerCategory)
            .Select(d => new ReportCenterHit
            {
                Title = d.Name,
                Subtitle = d.JobNo ?? d.Type,
                Date = D(d.DocumentDate),
                Url = $"/Documents/Download?id={d.Id}"
            })
            .ToListAsync(ct);

        return new ReportCenterCategory { Key = "document", Label = "Documents", Hits = rows };
    }
}
