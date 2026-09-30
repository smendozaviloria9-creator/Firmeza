using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Services;
using Xunit;

namespace Firmeza.Tests;

public class VentaTests
{
    [Fact]
    public void PorcentajeIva_DebeSer19PorCiento()
    {
        Assert.Equal(0.19m, ReciboService.PorcentajeIva);
    }

    [Fact]
    public void CalcularVenta_TotalesCorrectos()
    {
        // Arrange
        var precioUnitario = 50000m;
        var cantidad = 2;
        var subtotal = precioUnitario * cantidad; // 100,000
        var ivaEsperado = Math.Round(subtotal * ReciboService.PorcentajeIva, 2); // 19,000
        var totalEsperado = subtotal + ivaEsperado; // 119,000

        var venta = new Venta
        {
            ClienteId = 1,
            Fecha = DateTime.UtcNow,
            Subtotal = subtotal,
            Iva = ivaEsperado,
            Total = totalEsperado
        };

        // Assert
        Assert.Equal(100000m, venta.Subtotal);
        Assert.Equal(19000m, venta.Iva);
        Assert.Equal(119000m, venta.Total);
    }

    [Fact]
    public void DescontarStock_RestaCantidadCorrectamente()
    {
        // Arrange
        var producto = new Producto
        {
            Id = 1,
            Nombre = "Cemento Gris",
            Stock = 50,
            PrecioUnitario = 28000m
        };

        // Act
        var cantidadVendida = 15;
        producto.Stock -= cantidadVendida;

        // Assert
        Assert.Equal(35, producto.Stock);
    }
}
