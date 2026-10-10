namespace Firmeza.Application.DTOs.Vehiculos;

public class VehiculoDto
{
    public int Id { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string? Vin { get; set; }
    public string? Color { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal PrecioRentaDia { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}
