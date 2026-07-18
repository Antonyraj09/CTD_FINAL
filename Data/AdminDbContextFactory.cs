using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CTD_FINAL.Data;

/// <summary>
/// Lets `dotnet ef migrations add/script --context AdminDbContext` build a DbContext
/// without going through Program.cs/DI (there's no HTTP request scope at design time).
/// Reads the same appsettings*.json layering the app itself uses at runtime.
/// </summary>
public class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    public AdminDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("AdminConnection")
            ?? throw new InvalidOperationException("Connection string 'AdminConnection' not found for design-time context creation.");

        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AdminDbContext).Assembly.FullName))
            .Options;

        return new AdminDbContext(options);
    }
}
