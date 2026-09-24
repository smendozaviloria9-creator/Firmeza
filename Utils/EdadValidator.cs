namespace Firmeza.Web.Utils;


public static class EdadValidator
{
    public const int EdadMinima = 18;
    public const int EdadMaxima = 120;

    public static bool TryParseEdad(string? edadTexto, out int edad, out string? mensajeError)
    {
        edad = 0;
        mensajeError = null;

        if (string.IsNullOrWhiteSpace(edadTexto))
        {
            mensajeError = "Debes ingresar la edad del cliente.";
            return false;
        }

        try
        {
            // Conversión explícita que puede lanzar FormatException u OverflowException.
            edad = int.Parse(edadTexto.Trim());
        }
        catch (FormatException)
        {
            mensajeError = $"\"{edadTexto}\" no es un número entero válido. Ingresa solo dígitos, por ejemplo: 32.";
            return false;
        }
        catch (OverflowException)
        {
            mensajeError = "La edad ingresada es demasiado grande.";
            return false;
        }

        if (edad < EdadMinima || edad > EdadMaxima)
        {
            mensajeError = $"La edad debe estar entre {EdadMinima} y {EdadMaxima} años.";
            return false;
        }

        return true;
    }
}
