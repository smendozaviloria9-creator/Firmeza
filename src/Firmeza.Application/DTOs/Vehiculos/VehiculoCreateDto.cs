using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Vehiculos;

public class VehiculoCreateDto
{
    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(50)]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(50)]
    public string Modelo { get; set; } = string.Empty;

    [Required]
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

    [Range(0, double.MaxValue)]
    public decimal PrecioVenta { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PrecioRentaDia { get; set; }

    [StringLength(20)]
    public string Estado { get; set; } = "Disponible";
}
