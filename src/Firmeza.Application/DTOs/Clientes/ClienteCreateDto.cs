using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Clientes;

public class ClienteCreateDto
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento es obligatorio.")]
    [StringLength(20)]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress]
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es obligatorio.")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "La edad es obligatoria.")]
    [Range(18, 120)]
    public int Edad { get; set; }
}
