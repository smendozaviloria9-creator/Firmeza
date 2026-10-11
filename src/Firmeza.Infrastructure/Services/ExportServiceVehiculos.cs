using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public partial class ExportService
{
    public async Task<byte[]> ExportarVehiculosExcelAsync()
    {
        var vehiculos = await _context.Vehiculos.OrderBy(v => v.Marca).ThenBy(v => v.Modelo).ToListAsync();

        using var package = new ExcelPackage();
        var hoja = package.Workbook.Worksheets.Add("Vehiculos");

        string[] encabezados = { "Marca", "Modelo", "Año", "Placa", "VIN", "Color", "Tipo", "Kilometraje", "Precio venta", "Renta por día", "Estado" };
        for (int i = 0; i < encabezados.Length; i++)
            hoja.Cells[1, i + 1].Value = encabezados[i];
        hoja.Cells[1, 1, 1, encabezados.Length].Style.Font.Bold = true;

        int fila = 2;
        foreach (var v in vehiculos)
        {
            hoja.Cells[fila, 1].Value = v.Marca;
            hoja.Cells[fila, 2].Value = v.Modelo;
            hoja.Cells[fila, 3].Value = v.Anio;
            hoja.Cells[fila, 4].Value = v.Placa;
            hoja.Cells[fila, 5].Value = v.Vin;
            hoja.Cells[fila, 6].Value = v.Color;
            hoja.Cells[fila, 7].Value = v.Tipo;
            hoja.Cells[fila, 8].Value = v.Kilometraje;
            hoja.Cells[fila, 9].Value = v.PrecioVenta;
            hoja.Cells[fila, 10].Value = v.PrecioRentaDia;
            hoja.Cells[fila, 11].Value = v.Estado;
            fila++;
        }

        hoja.Cells[hoja.Dimension.Address].AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportarVehiculosPdfAsync()
    {
        var vehiculos = await _context.Vehiculos.OrderBy(v => v.Marca).ThenBy(v => v.Modelo).ToListAsync();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.Header().Text("Inventario de Vehículos").FontSize(18).Bold();

                page.Content().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(4);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(3);
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                    });

                    tabla.Header(header =>
                    {
                        header.Cell().Text("Vehículo").Bold();
                        header.Cell().Text("Placa").Bold();
                        header.Cell().Text("Tipo").Bold();
                        header.Cell().Text("Precio venta").Bold();
                        header.Cell().Text("Renta/día").Bold();
                        header.Cell().Text("Estado").Bold();
                    });

                    foreach (var v in vehiculos)
                    {
                        tabla.Cell().Text($"{v.Marca} {v.Modelo} {v.Anio}");
                        tabla.Cell().Text(v.Placa);
                        tabla.Cell().Text(v.Tipo);
                        tabla.Cell().Text(v.PrecioVenta.ToString("C"));
                        tabla.Cell().Text(v.PrecioRentaDia.ToString("C"));
                        tabla.Cell().Text(v.Estado);
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
