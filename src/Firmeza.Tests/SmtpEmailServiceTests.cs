using Firmeza.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Firmeza.Tests;

public class SmtpEmailServiceTests
{
    [Fact]
    public async Task EnviarCorreoAsync_SinPassword_NoLanzaExcepcion()
    {
        // Arrange: configuracion con password no configurada / placeholder
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "SmtpSettings:Host", "smtp.gmail.com" },
            { "SmtpSettings:Port", "587" },
            { "SmtpSettings:SenderEmail", "test@firmeza.com" },
            { "SmtpSettings:Password", "TU_APP_PASSWORD_GMAIL" },
            { "SmtpSettings:EnableSsl", "true" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var logger = NullLogger<SmtpEmailService>.Instance;
        var service = new SmtpEmailService(configuration, logger);

        // Act & Assert: no debe lanzar ninguna excepcion
        var exception = await Record.ExceptionAsync(() =>
            service.EnviarCorreoAsync("cliente@test.com", "Prueba", "<p>Hola</p>"));

        Assert.Null(exception);
    }
}
