using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Firmeza.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Servicios ----------

// Base de datos (PostgreSQL vía Npgsql). La estructura se crea con migraciones EF Core.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity con roles (Administrador / Cliente)
builder.Services.AddScoped<IExcelImportService, ExcelImportService>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// El panel Razor solo es accesible para el rol Administrador.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdministrador", policy => policy.RequireRole(Roles.Administrador));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Sebastian Mendoza");
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Firmeza.Web.Services.IExcelImportService, Firmeza.Web.Services.ExcelImportService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IReciboService, ReciboService>();

var app = builder.Build();
var carpetaRecibos = Path.Combine(app.Environment.WebRootPath, "recibos");
if (!Directory.Exists(carpetaRecibos))
{
    Directory.CreateDirectory(carpetaRecibos);
}
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ---------- Seed de roles y usuario administrador ----------
using (var scope = app.Services.CreateScope())
    
{
    await DbInitializer.SeedAsync(scope.ServiceProvider, app.Configuration);
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
