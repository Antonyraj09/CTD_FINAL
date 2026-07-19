using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace CTD_FINAL.Infrastructure.Provisioning;

/// <summary>
/// Executes a raw .sql script — as produced by `dotnet ef migrations script --idempotent`
/// — against an already-open SqlConnection, splitting on "GO" batch separators the same
/// way sqlcmd does. SqlCommand can only execute one batch at a time; EF's generated scripts
/// rely on GO to separate DDL that can't share a batch (e.g. a CREATE TABLE later ALTERed).
/// </summary>
public static class SqlScriptRunner
{
    private static readonly Regex BatchSeparator = new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static async Task ExecuteAsync(SqlConnection connection, string script, CancellationToken ct = default)
    {
        foreach (var batch in BatchSeparator.Split(script))
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;

            await using var command = connection.CreateCommand();
            command.CommandText = batch;
            command.CommandTimeout = 120;
            await command.ExecuteNonQueryAsync(ct);
        }
    }
}
