using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public partial class ExportService
{
    public async Task<byte[]> ExportarVentasVehiculosExcelAsync()
    {
        var ventas = await _context.VentasVehiculos
            .Include(v => v.Cliente)
            .Include(v => v.Vehiculo)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        using var package = new ExcelPackage();
        var hoja = package.Workbook.Worksheets.Add("VentasVehiculos");

        string[] encabezados = { "N° Venta", "Fecha", "Cliente", "Vehículo", "Placa", "Subtotal", "IVA", "Total" };
        for (int i = 0; i < encabezados.Length; i++)
            hoja.Cells[1, i + 1].Value = encabezados[i];
        hoja.Cells[1, 1, 1, encabezados.Length].Style.Font.Bold = true;

        int fila = 2;
        foreach (var v in ventas)
        {
            hoja.Cells[fila, 1].Value = v.Id;
            hoja.Cells[fila, 2].Value = v.Fecha.ToString("dd/MM/yyyy");
            hoja.Cells[fila, 3].Value = v.Cliente != null ? $"{v.Cliente.Nombres} {v.Cliente.Apellidos}" : "";
            hoja.Cells[fila, 4].Value = v.Vehiculo != null ? $"{v.Vehiculo.Marca} {v.Vehiculo.Modelo} {v.Vehiculo.Anio}" : "";
            hoja.Cells[fila, 5].Value = v.Vehiculo?.Placa;
            hoja.Cells[fila, 6].Value = v.Subtotal;
            hoja.Cells[fila, 7].Value = v.Iva;
            hoja.Cells[fila, 8].Value = v.Total;
            fila++;
        }

        hoja.Cells[hoja.Dimension.Address].AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportarVentasVehiculosPdfAsync()
    {
        var ventas = await _context.VentasVehiculos
            .Include(v => v.Cliente)
            .Include(v => v.Vehiculo)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.Header().Text("Ventas de Vehículos").FontSize(18).Bold();

                page.Content().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                        c.RelativeColumn(3);
                        c.RelativeColumn(4);
                        c.RelativeColumn(3);
                    });

                    tabla.Header(header =>
                    {
                        header.Cell().Text("N°").Bold();
                        header.Cell().Text("Fecha").Bold();
                        header.Cell().Text("Cliente").Bold();
                        header.Cell().Text("Vehículo").Bold();
                        header.Cell().Text("Total").Bold();
                    });

                    foreach (var v in ventas)
                    {
                        tabla.Cell().Text(v.Id.ToString());
                        tabla.Cell().Text(v.Fecha.ToString("dd/MM/yyyy"));
                        tabla.Cell().Text(v.Cliente != null ? $"{v.Cliente.Nombres} {v.Cliente.Apellidos}" : "");
                        tabla.Cell().Text(v.Vehiculo != null ? $"{v.Vehiculo.Marca} {v.Vehiculo.Modelo} ({v.Vehiculo.Placa})" : "");
                        tabla.Cell().Text(v.Total.ToString("C"));
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generado el ");
                    x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
            });
        });

        return documento.GeneratePdf();
    }
}
