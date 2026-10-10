namespace Firmeza.Application.DTOs.VentasVehiculos;

public class VentaVehiculoDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int VehiculoId { get; set; }
    public string VehiculoDescripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string? ReciboArchivo { get; set; }
}
