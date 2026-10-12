using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Data;


public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Requisito: la estructura de la BD se construye solo con migraciones EF Core.
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in new[] { Roles.Administrador, Roles.Cliente })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@firmeza.com";
        var adminPassword = configuration["AdminSeed:Password"] ?? "Admin123$";
        var adminNombre = configuration["AdminSeed:NombreCompleto"] ?? "Administrador Firmeza";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NombreCompleto = adminNombre
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Administrador);
            }
        }

        // Semilla de Repuestos (Autopartes) si la base de datos esta limpia
        if (!await context.Productos.AnyAsync())
        {
            var repuestos = new List<Producto>
            {
                new() { Nombre = "Pastillas de Freno Delanteras Cerámicas", Descripcion = "Mayor potencia de frenado y menor desgaste para sedanes y SUVs.", Categoria = "Frenos", UnidadMedida = "juego", PrecioUnitario = 180000m, Stock = 30, Activo = true },
                new() { Nombre = "Filtro de Aceite Sintético de Alto Rendimiento", Descripcion = "Filtración avanzada para protección del motor.", Categoria = "Motor", UnidadMedida = "unidad", PrecioUnitario = 45000m, Stock = 50, Activo = true },
                new() { Nombre = "Batería 12V 75Ah Libre de Mantenimiento", Descripcion = "Arranque en frío superior y máxima duración.", Categoria = "Eléctrico", UnidadMedida = "unidad", PrecioUnitario = 350000m, Stock = 15, Activo = true },
                new() { Nombre = "Amortiguador Delantero a Gas", Descripcion = "Estabilidad y confort superior en cualquier terreno.", Categoria = "Suspensión", UnidadMedida = "unidad", PrecioUnitario = 220000m, Stock = 20, Activo = true },
                new() { Nombre = "Kit de Embrague (Clutch) Reforzado", Descripcion = "Incluye disco, prensa y balinera para transmisión mecánica.", Categoria = "Transmisión", UnidadMedida = "kit", PrecioUnitario = 480000m, Stock = 12, Activo = true },
                new() { Nombre = "Bujías de Iridio (Juego x4)", Descripcion = "Chispa óptima y menor consumo de combustible.", Categoria = "Encendido", UnidadMedida = "juego", PrecioUnitario = 125000m, Stock = 40, Activo = true }
            };
            await context.Productos.AddRangeAsync(repuestos);
            await context.SaveChangesAsync();
        }

        // Semilla de Vehículos para Venta y Renta si la base de datos esta limpia
        if (!await context.Vehiculos.AnyAsync())
        {
            var vehiculos = new List<Vehiculo>
            {
                new() { Marca = "Toyota", Modelo = "Corolla Cross", Anio = 2024, Placa = "KHT345", Vin = "9BRBL340X0012345", Color = "Blanco Perlado", Tipo = "SUV", Kilometraje = 5200, PrecioVenta = 125000000m, PrecioRentaDia = 230000m, Estado = "Disponible" },
                new() { Marca = "Ford", Modelo = "Ranger XLT 4x4", Anio = 2023, Placa = "MNO789", Vin = "8AFBT340Y0098765", Color = "Gris Metálico", Tipo = "Camioneta", Kilometraje = 22000, PrecioVenta = 165000000m, PrecioRentaDia = 310000m, Estado = "Disponible" },
                new() { Marca = "Chevrolet", Modelo = "Onix Turbo", Anio = 2023, Placa = "FGH512", Vin = "9BGKS340Z0054321", Color = "Rojo Rubí", Tipo = "Sedán", Kilometraje = 14500, PrecioVenta = 68000000m, PrecioRentaDia = 150000m, Estado = "Disponible" },
                new() { Marca = "Mazda", Modelo = "CX-30 Grand Touring", Anio = 2024, Placa = "LKP990", Vin = "3MZDM340W0067890", Color = "Azul Marino", Tipo = "SUV", Kilometraje = 8000, PrecioVenta = 118000000m, PrecioRentaDia = 220000m, Estado = "Disponible" }
            };
            await context.Vehiculos.AddRangeAsync(vehiculos);
            await context.SaveChangesAsync();
        }
    }
}
