using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.ViewModels;

public class ClienteViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento es obligatorio.")]
    [StringLength(20)]
    [RegularExpression(@"^[0-9]{5,20}$", ErrorMessage = "El documento debe contener solo números (5 a 20 dígitos).")]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^[0-9]{7,10}$", ErrorMessage = "El teléfono debe contener solo números (7 a 10 dígitos)")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; }
    

    [StringLength(200)]
    public string? Direccion { get; set; }

   
    [Required(ErrorMessage = "La edad es obligatoria.")]
    [Display(Name = "Edad")]
    public string EdadTexto { get; set; } = string.Empty;
}
