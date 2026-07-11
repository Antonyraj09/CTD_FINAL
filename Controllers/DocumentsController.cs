using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Controllers;

[Authorize]
[RequirePermission(PermissionKeys.TrackingView)]
public class DocumentsController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly IAuditService _auditService;
    private readonly IWebHostEnvironment _env;

    public DocumentsController(IDocumentService documentService, IAuditService auditService, IWebHostEnvironment env)
    {
        _documentService = documentService;
        _auditService = auditService;
        _env = env;
    }

    private string CurrentUserName => User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "System";

    /// <summary>
    /// Uploads are stored outside wwwroot so UseStaticFiles() can never serve them directly —
    /// every download must go through the [Authorize]-protected Download action below.
    /// </summary>
    private string UploadsRoot => Path.Combine(_env.ContentRootPath, "App_Data", "uploads", "documents");

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt" };

    private const long MaxUploadBytes = 20 * 1024 * 1024;

    public IActionResult Index()
    {
        ViewData["Title"] = "Document Attachment & PDF Archive";
        ViewData["Breadcrumb"] = "CTD Suite / Operations";
        ViewData["ActiveNav"] = "documents";
        ViewData["ActiveModule"] = "jobs";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Table(string? q, string? type, int page = 1)
    {
        var result = await _documentService.SearchAsync(q, type, page, 20);
        return PartialView("_DocumentsTable", result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.DocumentUpload)]
    public async Task<IActionResult> Upload(string name, string type, string? jobNo, IFormFile? file)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Enter a document name." });

        string? storagePath = null;
        if (file is { Length: > 0 })
        {
            if (file.Length > MaxUploadBytes)
                return Json(new { success = false, message = "File exceeds the 20 MB upload limit." });

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
                return Json(new { success = false, message = "Unsupported file type. Allowed: PDF, JPG, PNG, DOC(X), XLS(X), CSV, TXT." });

            Directory.CreateDirectory(UploadsRoot);
            var safeName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(UploadsRoot, safeName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);
            storagePath = safeName;
        }

        var doc = await _documentService.RegisterAsync(
            string.IsNullOrWhiteSpace(jobNo) ? "—" : jobNo, null, type, CurrentUserName,
            systemGenerated: false, storagePath: storagePath, explicitName: name, fileSizeBytes: file?.Length);

        await _auditService.LogAsync(AuditActionType.Created, CurrentUserName, jobNo, detail: $"Document '{name}' uploaded to archive");
        return Json(new { success = true, message = name });
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        var record = await _documentService.GetByIdAsync(id);
        if (record is null) return NotFound();

        if (!string.IsNullOrEmpty(record.StoragePath))
        {
            // Storage path is always a bare generated filename (see Upload above); strip any
            // directory component defensively before combining, so a corrupted/legacy value
            // can never escape the uploads root.
            var fullPath = Path.Combine(UploadsRoot, Path.GetFileName(record.StoragePath));
            if (System.IO.File.Exists(fullPath))
                return PhysicalFile(fullPath, "application/octet-stream", record.Name);
        }

        // System-generated documents have no physical file — serve a text summary instead.
        var content = $"CTD Management System — Document Record\n\nDocument: {record.Name}\nType: {record.Type}\nJob: {record.JobNo}\nUploaded by: {record.UploadedBy}\nDate: {record.DocumentDate:d MMM yyyy}\n\nThis is a system-generated record. Open the source job to regenerate the live document.";
        return File(System.Text.Encoding.UTF8.GetBytes(content), "text/plain", record.Name.Replace(".pdf", ".txt"));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.DocumentUpload)]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await _documentService.GetByIdAsync(id);
        var deleted = await _documentService.DeleteAsync(id);
        if (!deleted) return Json(new { success = false, message = "Document not found." });

        if (!string.IsNullOrEmpty(record?.StoragePath))
        {
            var fullPath = Path.Combine(UploadsRoot, Path.GetFileName(record.StoragePath));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
        }

        await _auditService.LogAsync(AuditActionType.Deleted, CurrentUserName, detail: $"Document #{id} removed from archive");
        return Json(new { success = true, message = "Removed from archive" });
    }
}
