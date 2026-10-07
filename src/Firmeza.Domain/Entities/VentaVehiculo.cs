using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Firmeza.Domain.Entities;

public class VentaVehiculo
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Required]
    public int VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(12,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Iva { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Total { get; set; }

    public string? ReciboArchivo { get; set; }
}
