using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Ventas;

public class VentaCreateDto
{
    [Required(ErrorMessage = "Debes seleccionar un cliente.")]
    public int ClienteId { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Debes agregar al menos un producto.")]
    [MinLength(1, ErrorMessage = "Debes agregar al menos un producto.")]
    public List<DetalleVentaCreateDto> Items { get; set; } = new();
}

public class DetalleVentaCreateDto
{
    [Required(ErrorMessage = "Selecciona un producto.")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "Indica la cantidad.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}
