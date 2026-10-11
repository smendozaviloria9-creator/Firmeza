using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public partial class ExportService
{
    public async Task<byte[]> ExportarRentasExcelAsync()
    {
        var rentas = await _context.Rentas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo)
            .OrderByDescending(r => r.FechaInicio)
            .ToListAsync();

        using var package = new ExcelPackage();
        var hoja = package.Workbook.Worksheets.Add("Rentas");

        string[] encabezados = { "N° Renta", "Cliente", "Vehículo", "Placa", "Inicio", "Fin", "Tarifa por día", "Total", "Estado" };
        for (int i = 0; i < encabezados.Length; i++)
            hoja.Cells[1, i + 1].Value = encabezados[i];
        hoja.Cells[1, 1, 1, encabezados.Length].Style.Font.Bold = true;

        int fila = 2;
        foreach (var r in rentas)
        {
            hoja.Cells[fila, 1].Value = r.Id;
            hoja.Cells[fila, 2].Value = r.Cliente != null ? $"{r.Cliente.Nombres} {r.Cliente.Apellidos}" : "";
            hoja.Cells[fila, 3].Value = r.Vehiculo != null ? $"{r.Vehiculo.Marca} {r.Vehiculo.Modelo} {r.Vehiculo.Anio}" : "";
            hoja.Cells[fila, 4].Value = r.Vehiculo?.Placa;
            hoja.Cells[fila, 5].Value = r.FechaInicio.ToString("dd/MM/yyyy");
            hoja.Cells[fila, 6].Value = r.FechaFin.ToString("dd/MM/yyyy");
            hoja.Cells[fila, 7].Value = r.TarifaDia;
            hoja.Cells[fila, 8].Value = r.Total;
            hoja.Cells[fila, 9].Value = r.Estado;
            fila++;
        }

        hoja.Cells[hoja.Dimension.Address].AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportarRentasPdfAsync()
    {
        var rentas = await _context.Rentas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo)
            .OrderByDescending(r => r.FechaInicio)
            .ToListAsync();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.Header().Text("Rentas de Vehículos").FontSize(18).Bold();

                page.Content().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);
                        c.RelativeColumn(3);
                        c.RelativeColumn(4);
                        c.RelativeColumn(4);
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                    });

                    tabla.Header(header =>
                    {
                        header.Cell().Text("N°").Bold();
                        header.Cell().Text("Cliente").Bold();
                        header.Cell().Text("Vehículo").Bold();
                        header.Cell().Text("Periodo").Bold();
                        header.Cell().Text("Total").Bold();
                        header.Cell().Text("Estado").Bold();
                    });

                    foreach (var r in rentas)
                    {
                        tabla.Cell().Text(r.Id.ToString());
                        tabla.Cell().Text(r.Cliente != null ? $"{r.Cliente.Nombres} {r.Cliente.Apellidos}" : "");
                        tabla.Cell().Text(r.Vehiculo != null ? $"{r.Vehiculo.Marca} {r.Vehiculo.Modelo} ({r.Vehiculo.Placa})" : "");
                        tabla.Cell().Text($"{r.FechaInicio:dd/MM/yyyy} - {r.FechaFin:dd/MM/yyyy}");
                        tabla.Cell().Text(r.Total.ToString("C"));
                        tabla.Cell().Text(r.Estado);
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
