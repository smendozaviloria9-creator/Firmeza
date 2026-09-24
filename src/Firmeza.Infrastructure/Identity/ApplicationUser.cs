using Microsoft.AspNetCore.Identity;

using Firmeza.Domain.Entities;

namespace Firmeza.Infrastructure.Identity;


public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;

    // Si el usuario es de rol Cliente, se puede vincular a su ficha de Cliente.
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
}
