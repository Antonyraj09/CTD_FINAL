namespace CTD_FINAL.Interfaces;

public interface IPermissionService
{
    Task<bool> IsAllowedAsync(string role, string moduleKey, CancellationToken ct = default);
    Task<Dictionary<string, Dictionary<string, bool>>> GetMatrixAsync(CancellationToken ct = default);
    Task SaveMatrixAsync(Dictionary<string, Dictionary<string, bool>> matrix, CancellationToken ct = default);
}
