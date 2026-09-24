using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.ViewModels;

public class ProductoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(80)]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(30)]
    [Display(Name = "Unidad de medida")]
    public string UnidadMedida { get; set; } = "unidad";

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0.")]
    [Display(Name = "Precio unitario")]
    public decimal PrecioUnitario { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;
}
