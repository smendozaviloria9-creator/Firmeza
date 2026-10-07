using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Firmeza.Domain.Entities;

public class Renta
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Required]
    public int VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal TarifaDia { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Total { get; set; }

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Activa";

    public string? ContratoArchivo { get; set; }
}
