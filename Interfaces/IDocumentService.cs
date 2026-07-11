using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Interfaces;

public interface IDocumentService
{
    Task<PagedResult<GeneratedDocument>> SearchAsync(string? query, string? type, int page, int pageSize, CancellationToken ct = default);
    Task<GeneratedDocument?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<GeneratedDocument> RegisterAsync(string jobNo, int? jobId, string type, string uploadedBy, bool systemGenerated, string? storagePath = null, string? explicitName = null, long? fileSizeBytes = null, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
