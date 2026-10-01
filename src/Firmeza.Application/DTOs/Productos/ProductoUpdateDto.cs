using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Productos;

public class ProductoUpdateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Required]
    [StringLength(80)]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(30)]
    public string UnidadMedida { get; set; } = "unidad";

    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal PrecioUnitario { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    public bool Activo { get; set; }
}
