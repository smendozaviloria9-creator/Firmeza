using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.ViewModels;

public class VentaViewModel
{
    [Required(ErrorMessage = "Debes seleccionar un cliente.")]
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [Display(Name = "Fecha")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public List<DetalleVentaItemViewModel> Items { get; set; } = new();
}

public class DetalleVentaItemViewModel
{
    [Required(ErrorMessage = "Selecciona un producto.")]
    [Display(Name = "Producto")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "Indica la cantidad.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}