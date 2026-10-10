using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.VentasVehiculos;

public class VentaVehiculoCreateDto
{
    [Required(ErrorMessage = "Debes seleccionar un cliente.")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Debes seleccionar un vehiculo.")]
    public int VehiculoId { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
