using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Firmeza.Domain.Entities;
using Firmeza.Application.Interfaces;

namespace Firmeza.Infrastructure.Services;

public class ReciboService : IReciboService
{
    // IVA fijo del 19% — ajusta este valor si tu negocio usa otro porcentaje.
    public const decimal PorcentajeIva = 0.19m;

    public string GenerarRecibo(Venta venta, string carpetaDestino)
    {
        var nombreArchivo = $"Recibo_{venta.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Firmeza").FontSize(20).Bold();
                    col.Item().Text("Materiales de construcción");
                    col.Item().PaddingTop(10).Text($"Recibo de venta N° {venta.Id}").FontSize(14).Bold();
                    col.Item().Text($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
                });

                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Item().Text("Datos del cliente").Bold();
                    col.Item().Text($"Nombre: {venta.Cliente?.Nombres} {venta.Cliente?.Apellidos}");
                    col.Item().Text($"Documento: {venta.Cliente?.Documento}");
                    col.Item().Text($"Teléfono: {venta.Cliente?.Telefono}");

                    col.Item().PaddingTop(15).Text("Productos").Bold();

                    col.Item().PaddingTop(5).Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        tabla.Header(header =>
                        {
                            header.Cell().Text("Producto").Bold();
                            header.Cell().Text("Cant.").Bold();
                            header.Cell().Text("Precio").Bold();
                            header.Cell().Text("Subtotal").Bold();
                        });

                        foreach (var d in venta.Detalles)
                        {
                            tabla.Cell().Text(d.Producto?.Nombre ?? "");
                            tabla.Cell().Text(d.Cantidad.ToString());
                            tabla.Cell().Text(d.PrecioUnitario.ToString("C"));
                            tabla.Cell().Text(d.Subtotal.ToString("C"));
                        }
                    });

                    col.Item().PaddingTop(15).AlignRight().Column(totales =>
                    {
                        totales.Item().Text($"Subtotal: {venta.Subtotal:C}");
                        totales.Item().Text($"IVA (19%): {venta.Iva:C}");
                        totales.Item().Text($"Total: {venta.Total:C}").Bold().FontSize(13);
                    });
                });

                page.Footer().AlignCenter().Text("Gracias por su compra — Firmeza").FontSize(9);
            });
        });

        documento.GeneratePdf(rutaCompleta);
        return nombreArchivo;
    }
}
