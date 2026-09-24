using Microsoft.AspNetCore.Identity;

namespace Firmeza.Web.Models;


public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;

    // Si el usuario es de rol Cliente, se puede vincular a su ficha de Cliente.
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
}
