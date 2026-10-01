namespace Firmeza.Application.DTOs.Productos;

public class ProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
