namespace Firmeza.Application.DTOs.Rentas;

public class RentaDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int VehiculoId { get; set; }
    public string VehiculoDescripcion { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TarifaDia { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? ContratoArchivo { get; set; }
}
