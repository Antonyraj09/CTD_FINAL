using CTD_FINAL.Models.ReportCenter;

namespace CTD_FINAL.Interfaces;

/// <summary>Cross-entity "search everything" lookup backing the Report Center screen — given
/// a free-text term, matches it against the key fields of every major record type in the
/// tenant's own database and returns the hits grouped by type.</summary>
public interface IReportCenterService
{
    /// <summary>Only searches the entity types named in allowedCategoryKeys — the caller
    /// (the controller) is responsible for computing that set from the current user's actual
    /// permissions, so a category the user can't otherwise view never appears here either.</summary>
    Task<List<ReportCenterCategory>> SearchAsync(string term, ISet<string> allowedCategoryKeys, CancellationToken ct = default);
}
