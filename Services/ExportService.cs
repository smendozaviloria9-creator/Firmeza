using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportarProductosExcelAsync();
        Task<byte[]> ExportarProductosPdfAsync();
        Task<byte[]> ExportarClientesExcelAsync();
        Task<byte[]> ExportarClientesPdfAsync();
        Task<byte[]> ExportarVentasExcelAsync();
        Task<byte[]> ExportarVentasPdfAsync();
    }

    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------- PRODUCTOS ----------

        public async Task<byte[]> ExportarProductosExcelAsync()
        {
            var productos = await _context.Productos.OrderBy(p => p.Nombre).ToListAsync();

            using var package = new ExcelPackage();
            var hoja = package.Workbook.Worksheets.Add("Productos");

            string[] encabezados = { "Nombre", "Descripción", "Categoría", "Unidad", "Precio", "Stock", "Activo" };
            for (int i = 0; i < encabezados.Length; i++)
                hoja.Cells[1, i + 1].Value = encabezados[i];
            hoja.Cells[1, 1, 1, encabezados.Length].Style.Font.Bold = true;

            int fila = 2;
            foreach (var p in productos)
            {
                hoja.Cells[fila, 1].Value = p.Nombre;
                hoja.Cells[fila, 2].Value = p.Descripcion;
                hoja.Cells[fila, 3].Value = p.Categoria;
                hoja.Cells[fila, 4].Value = p.UnidadMedida;
                hoja.Cells[fila, 5].Value = p.PrecioUnitario;
                hoja.Cells[fila, 6].Value = p.Stock;
                hoja.Cells[fila, 7].Value = p.Activo ? "Sí" : "No";
                fila++;
            }

            hoja.Cells[hoja.Dimension.Address].AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task<byte[]> ExportarProductosPdfAsync()
        {
            var productos = await _context.Productos.OrderBy(p => p.Nombre).ToListAsync();

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.Header().Text("Listado de Productos").FontSize(18).Bold();

                    page.Content().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                        });

                        tabla.Header(header =>
                        {
                            header.Cell().Text("Nombre").Bold();
                            header.Cell().Text("Categoría").Bold();
                            header.Cell().Text("Unidad").Bold();
                            header.Cell().Text("Precio").Bold();
                            header.Cell().Text("Stock").Bold();
                        });

                        foreach (var p in productos)
                        {
                            tabla.Cell().Text(p.Nombre);
                            tabla.Cell().Text(p.Categoria);
                            tabla.Cell().Text(p.UnidadMedida);
                            tabla.Cell().Text(p.PrecioUnitario.ToString("C"));
                            tabla.Cell().Text(p.Stock.ToString());
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

        // ---------- CLIENTES ----------

        public async Task<byte[]> ExportarClientesExcelAsync()
        {
            var clientes = await _context.Clientes.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).ToListAsync();

            using var package = new ExcelPackage();
            var hoja = package.Workbook.Worksheets.Add("Clientes");

            string[] encabezados = { "Nombres", "Apellidos", "Documento", "Correo", "Teléfono", "Dirección", "Edad" };
            for (int i = 0; i < encabezados.Length; i++)
                hoja.Cells[1, i + 1].Value = encabezados[i];
            hoja.Cells[1, 1, 1, encabezados.Length].Style.Font.Bold = true;

            int fila = 2;
            foreach (var c in clientes)
            {
                hoja.Cells[fila, 1].Value = c.Nombres;
                hoja.Cells[fila, 2].Value = c.Apellidos;
                hoja.Cells[fila, 3].Value = c.Documento;
                hoja.Cells[fila, 4].Value = c.Correo;
                hoja.Cells[fila, 5].Value = c.Telefono;
                hoja.Cells[fila, 6].Value = c.Direccion;
                hoja.Cells[fila, 7].Value = c.Edad;
                fila++;
            }

            hoja.Cells[hoja.Dimension.Address].AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task<byte[]> ExportarClientesPdfAsync()
        {
            var clientes = await _context.Clientes.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).ToListAsync();

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.Header().Text("Listado de Clientes").FontSize(18).Bold();

                    page.Content().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                        });

                        tabla.Header(header =>
                        {
                            header.Cell().Text("Nombre completo").Bold();
                            header.Cell().Text("Documento").Bold();
                            header.Cell().Text("Correo").Bold();
                            header.Cell().Text("Teléfono").Bold();
                        });

                        foreach (var c in clientes)
                        {
                            tabla.Cell().Text($"{c.Nombres} {c.Apellidos}");
                            tabla.Cell().Text(c.Documento);
                            tabla.Cell().Text(c.Correo);
                            tabla.Cell().Text(c.Telefono);
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

        // ---------- VENTAS ----------

        public async Task<byte[]> ExportarVentasExcelAsync()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            using var package = new ExcelPackage();
            var hoja = package.Workbook.Worksheets.Add("Ventas");

            string[] encabezados = { "N° Venta", "Fecha", "Cliente", "Producto", "Cantidad", "Precio Unitario", "Subtotal", "Total Venta" };
            for (int i = 0; i < encabezados.Length; i++)
                hoja.Cells[1, i + 1].Value = encabezados[i];
            hoja.Cells[1, 1, 1, encabezados.Length].Style.Font.Bold = true;

            int fila = 2;
            foreach (var v in ventas)
            {
                foreach (var d in v.Detalles)
                {
                    hoja.Cells[fila, 1].Value = v.Id;
                    hoja.Cells[fila, 2].Value = v.Fecha.ToString("dd/MM/yyyy");
                    hoja.Cells[fila, 3].Value = v.Cliente != null ? $"{v.Cliente.Nombres} {v.Cliente.Apellidos}" : "";
                    hoja.Cells[fila, 4].Value = d.Producto?.Nombre;
                    hoja.Cells[fila, 5].Value = d.Cantidad;
                    hoja.Cells[fila, 6].Value = d.PrecioUnitario;
                    hoja.Cells[fila, 7].Value = d.Subtotal;
                    hoja.Cells[fila, 8].Value = v.Total;
                    fila++;
                }
            }

            hoja.Cells[hoja.Dimension.Address].AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task<byte[]> ExportarVentasPdfAsync()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.Header().Text("Listado de Ventas").FontSize(18).Bold();

                    page.Content().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                        });

                        tabla.Header(header =>
                        {
                            header.Cell().Text("N°").Bold();
                            header.Cell().Text("Fecha").Bold();
                            header.Cell().Text("Cliente").Bold();
                            header.Cell().Text("Total").Bold();
                        });

                        foreach (var v in ventas)
                        {
                            tabla.Cell().Text(v.Id.ToString());
                            tabla.Cell().Text(v.Fecha.ToString("dd/MM/yyyy"));
                            tabla.Cell().Text(v.Cliente != null ? $"{v.Cliente.Nombres} {v.Cliente.Apellidos}" : "");
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
}