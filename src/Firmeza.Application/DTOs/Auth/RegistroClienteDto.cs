using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Auth;

public class RegistroClienteDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string NombreCompleto { get; set; } = string.Empty;
}
