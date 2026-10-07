using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Data;


public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<VentaVehiculo> VentasVehiculos => Set<VentaVehiculo>();
    public DbSet<Renta> Rentas => Set<Renta>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cliente>()
            .HasIndex(c => c.Documento)
            .IsUnique();

        builder.Entity<Cliente>()
            .HasIndex(c => c.Correo)
            .IsUnique();

        builder.Entity<Venta>()
            .HasOne(v => v.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DetalleVenta>()
            .HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto)
            .WithMany(p => p.DetallesVenta)
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Vehiculo>()
            .HasIndex(v => v.Placa)
            .IsUnique();

        builder.Entity<VentaVehiculo>()
            .HasOne(vv => vv.Cliente)
            .WithMany(c => c.VentasVehiculos)
            .HasForeignKey(vv => vv.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VentaVehiculo>()
            .HasOne(vv => vv.Vehiculo)
            .WithOne(v => v.VentaVehiculo)
            .HasForeignKey<VentaVehiculo>(vv => vv.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Renta>()
            .HasOne(r => r.Cliente)
            .WithMany(c => c.Rentas)
            .HasForeignKey(r => r.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Renta>()
            .HasOne(r => r.Vehiculo)
            .WithMany(v => v.Rentas)
            .HasForeignKey(r => r.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
