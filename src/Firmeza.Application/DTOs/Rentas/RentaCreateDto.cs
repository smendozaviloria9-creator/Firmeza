using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Rentas;

public class RentaCreateDto
{
    [Required(ErrorMessage = "Debes seleccionar un cliente.")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Debes seleccionar un vehiculo.")]
    public int VehiculoId { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateTime FechaFin { get; set; }
}
