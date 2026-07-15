using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Helpers;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _context;

    public DocumentService(AppDbContext context) => _context = context;

    public async Task<PagedResult<GeneratedDocument>> SearchAsync(string? query, string? type, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _context.GeneratedDocuments.AsNoTracking().OrderByDescending(d => d.DocumentDate).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(d => d.Name.Contains(query) || (d.JobNo != null && d.JobNo.Contains(query)) || d.Type.Contains(query));
        if (!string.IsNullOrWhiteSpace(type))
            q = q.Where(d => d.Type == type);

        return await q.ToPagedResultAsync(page, pageSize, ct);
    }

    public async Task<GeneratedDocument?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.GeneratedDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<GeneratedDocument> RegisterAsync(string jobNo, int? jobId, string type, string uploadedBy, bool systemGenerated,
        string? storagePath = null, string? explicitName = null, long? fileSizeBytes = null, CancellationToken ct = default)
    {
        var name = !string.IsNullOrWhiteSpace(explicitName)
            ? (Path.HasExtension(explicitName) ? explicitName : explicitName + (systemGenerated ? ".pdf" : Path.GetExtension(storagePath ?? "")))
            : $"{type.Replace(" ", "_")}_{jobNo}{(systemGenerated ? ".pdf" : Path.GetExtension(storagePath ?? ""))}";

        var doc = new GeneratedDocument
        {
            Name = name,
            Type = type,
            CtdJobId = jobId,
            JobNo = jobNo,
            UploadedBy = uploadedBy,
            DocumentDate = DateTime.UtcNow,
            Size = fileSizeBytes.HasValue ? $"{fileSizeBytes.Value / 1024.0 / 1024.0:F1} MB" : null,
            SystemGenerated = systemGenerated,
            StoragePath = storagePath
        };
        _context.GeneratedDocuments.Add(doc);
        await _context.SaveChangesAsync(ct);
        return doc;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var doc = await _context.GeneratedDocuments.FindAsync(new object?[] { id }, ct);
        if (doc is null) return false;
        _context.GeneratedDocuments.Remove(doc);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
