using ERecruitment.Web.Services;
using ERecruitment.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ERecruitment.Web.Storage;
using ERecruitment.Web.Background;
using ERecruitment.Web.Notifications;
using ERecruitment.Web.Security;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5050", "https://localhost:5051");


builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ERecruitment.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Domain persistence (Phase 2): EF Core SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sql =>
        sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));

    // Enable query logging in Development to verify N+1 elimination
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

builder.Services.Configure<AuditOptions>(builder.Configuration.GetSection("Audit"));

// Phase 2 Architecture: Optimized service layer (3 focused services)
builder.Services.AddScoped<IRecruitmentRepository, EfRecruitmentRepository>();
builder.Services.AddScoped<IApplicantManagementService, ApplicantManagementService>();
builder.Services.AddScoped<IApplicationWorkflowService, ApplicationWorkflowService>();
builder.Services.AddScoped<IAdministrationService, AdministrationService>();
builder.Services.AddScoped<ICurrentApplicant, CurrentApplicantAccessor>();
builder.Services.AddHttpContextAccessor();

// Identity with EF Core SQL Server (changed from InMemory for persistence)
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
{
    options.UseSqlServer(connectionString);

    // Enable query logging in Development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false; // can enable later
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".ERecruitment.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// Phase 3: CV storage and background parsing
builder.Services.AddSingleton<IFileScanner, MagicHeaderFileScanner>();
builder.Services.AddSingleton<ICvStorage, LocalDiskCvStorage>();
builder.Services.AddSingleton<ICvParseJobQueue, InMemoryCvParseJobQueue>();
builder.Services.AddHostedService<CvParseWorker>();

// Email sender (use SMTP if configured, otherwise console)
if (!string.IsNullOrWhiteSpace(builder.Configuration["Smtp:Host"]))
{
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
}
else
{
    builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();
}
builder.Services.Decorate<IEmailSender, LoggingEmailSenderDecorator>();
builder.Services.AddSingleton<IEmailTemplateRenderer, RazorEmailTemplateRenderer>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure Identity database is migrated
using (var scope = app.Services.CreateScope())
{
    var identityDb = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
    identityDb.Database.Migrate();
}

// Seed Identity (Admin role/user)
await ERecruitment.Web.Data.IdentitySeeder.SeedAsync(app.Services);

// Ensure domain database is migrated
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    ERecruitment.Web.Data.DomainSeeder.EnsureSeeded(db);
}

app.Run();
