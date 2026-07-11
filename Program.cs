using CTD_FINAL.Data;
using CTD_FINAL.Data.Seed;
using CTD_FINAL.Entities;
using CTD_FINAL.Infrastructure;
using CTD_FINAL.Infrastructure.Identity;
using CTD_FINAL.Infrastructure.Middleware;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Repositories;
using CTD_FINAL.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ctdsuite-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30));

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("AppOptions"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMemoryCache();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IMasterCrudService<>), typeof(MasterCrudService<>));
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<INumberSequenceService, NumberSequenceService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAlertSender, LoggedAlertSender>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IJobIsneService, JobIsneService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPartyService, PartyService>();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppUserClaimsPrincipalFactory>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("AppOptions:SessionTimeoutMinutes", 30));
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAntiforgery(options =>
{
    // Lets AJAX POSTs with a JSON body (no form fields) supply the token via header instead.
    options.HeaderName = "RequestVerificationToken";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseGlobalExceptionMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

app.Run();
