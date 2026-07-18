using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CTD_FINAL.Data;

/// <summary>
/// Lets `dotnet ef migrations add/script` (default context) build an AppDbContext without
/// going through Program.cs/DI. Required once AppDbContext's real DI registration became
/// tenant-aware (resolves ITenantContextAccessor from the current HTTP request's scope) —
/// design-time tooling has no request scope, so it needs this explicit bypass instead.
/// Uses ConnectionStrings:DefaultConnection purely as the design-time/local-dev schema
/// target; it is no longer used at runtime once tenant resolution is wired up.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found for design-time context creation.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
