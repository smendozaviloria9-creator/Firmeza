namespace Firmeza.Application.Interfaces;

public interface IEmailService
{
    Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml);
}
