using System.ComponentModel.DataAnnotations.Schema;

namespace Firmeza.Web.Models;

public class DetalleVenta
{
    public int Id { get; set; }

    [ForeignKey(nameof(Venta))]
    public int VentaId { get; set; }
    public Venta? Venta { get; set; }

    [ForeignKey(nameof(Producto))]
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int Cantidad { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Subtotal { get; set; }
}
