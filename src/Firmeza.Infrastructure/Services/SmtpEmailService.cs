using System.Net;
using System.Net.Mail;
using Firmeza.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Firmeza.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var host = _configuration["SmtpSettings:Host"] ?? "smtp.gmail.com";
        var portStr = _configuration["SmtpSettings:Port"] ?? "587";
        int.TryParse(portStr, out var port);
        if (port == 0) port = 587;

        var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "notificaciones@firmeza.com";
        var senderName = _configuration["SmtpSettings:SenderName"] ?? "Firmeza Materiales";
        var password = _configuration["SmtpSettings:Password"];
        var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"] ?? "true");

        // Si no se ha configurado la contraseña real de aplicación, registramos log y evitamos bloquear la operación
        if (string.IsNullOrWhiteSpace(password) || password.StartsWith("TU_APP_PASSWORD"))
        {
            _logger.LogWarning("Servicio de correo: Contraseña SMTP no configurada en SmtpSettings. Correo simulado hacia {Destinatario}: {Asunto}", destinatario, asunto);
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mailMessage.To.Add(destinatario);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Correo enviado exitosamente a {Destinatario} con asunto: {Asunto}", destinatario, asunto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo SMTP a {Destinatario}", destinatario);
            // No propagamos la excepción para no interrumpir el flujo principal de compra o registro
        }
    }
}
