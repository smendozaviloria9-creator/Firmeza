using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Firmeza.Domain.Entities;

public class Vehiculo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(50)]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(50)]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El año es obligatorio.")]
    [Range(1900, 2100)]
    public int Anio { get; set; }

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(20)]
    public string Placa { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Vin { get; set; }

    [StringLength(30)]
    public string? Color { get; set; }

    [Required(ErrorMessage = "El tipo de vehiculo es obligatorio.")]
    [StringLength(30)]
    public string Tipo { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Kilometraje { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    [Range(0, double.MaxValue)]
    public decimal PrecioVenta { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    [Range(0, double.MaxValue)]
    public decimal PrecioRentaDia { get; set; }

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "Disponible";

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public VentaVehiculo? VentaVehiculo { get; set; }
    public ICollection<Renta> Rentas { get; set; } = new List<Renta>();
}
