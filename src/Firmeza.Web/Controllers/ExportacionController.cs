using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Firmeza.Application.Interfaces;

namespace Firmeza.Web.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class ExportacionController : Controller
    {
        private readonly IExportService _exportService;

        public ExportacionController(IExportService exportService)
        {
            _exportService = exportService;
        }

        public async Task<IActionResult> ProductosExcel()
        {
            var bytes = await _exportService.ExportarProductosExcelAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Productos_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ProductosPdf()
        {
            var bytes = await _exportService.ExportarProductosPdfAsync();
            return File(bytes, "application/pdf", $"Productos_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ClientesExcel()
        {
            var bytes = await _exportService.ExportarClientesExcelAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Clientes_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> ClientesPdf()
        {
            var bytes = await _exportService.ExportarClientesPdfAsync();
            return File(bytes, "application/pdf", $"Clientes_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> VentasExcel()
        {
            var bytes = await _exportService.ExportarVentasExcelAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Ventas_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> VentasPdf()
        {
            var bytes = await _exportService.ExportarVentasPdfAsync();
            return File(bytes, "application/pdf", $"Ventas_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}