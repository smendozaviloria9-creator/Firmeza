using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ExportacionController : ControllerBase
{
    private const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string TipoPdf = "application/pdf";

    private readonly IExportService _exportService;

    public ExportacionController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpGet("productos/excel")]
    public async Task<IActionResult> ProductosExcel() => File(await _exportService.ExportarProductosExcelAsync(), TipoExcel, $"Productos_{DateTime.Now:yyyyMMdd}.xlsx");

    [HttpGet("productos/pdf")]
    public async Task<IActionResult> ProductosPdf() => File(await _exportService.ExportarProductosPdfAsync(), TipoPdf, $"Productos_{DateTime.Now:yyyyMMdd}.pdf");

    [HttpGet("clientes/excel")]
    public async Task<IActionResult> ClientesExcel() => File(await _exportService.ExportarClientesExcelAsync(), TipoExcel, $"Clientes_{DateTime.Now:yyyyMMdd}.xlsx");

    [HttpGet("clientes/pdf")]
    public async Task<IActionResult> ClientesPdf() => File(await _exportService.ExportarClientesPdfAsync(), TipoPdf, $"Clientes_{DateTime.Now:yyyyMMdd}.pdf");

    [HttpGet("ventas/excel")]
    public async Task<IActionResult> VentasExcel() => File(await _exportService.ExportarVentasExcelAsync(), TipoExcel, $"Ventas_{DateTime.Now:yyyyMMdd}.xlsx");

    [HttpGet("ventas/pdf")]
    public async Task<IActionResult> VentasPdf() => File(await _exportService.ExportarVentasPdfAsync(), TipoPdf, $"Ventas_{DateTime.Now:yyyyMMdd}.pdf");

    [HttpGet("vehiculos/excel")]
    public async Task<IActionResult> VehiculosExcel() => File(await _exportService.ExportarVehiculosExcelAsync(), TipoExcel, $"Vehiculos_{DateTime.Now:yyyyMMdd}.xlsx");

    [HttpGet("vehiculos/pdf")]
    public async Task<IActionResult> VehiculosPdf() => File(await _exportService.ExportarVehiculosPdfAsync(), TipoPdf, $"Vehiculos_{DateTime.Now:yyyyMMdd}.pdf");

    [HttpGet("ventas-vehiculos/excel")]
    public async Task<IActionResult> VentasVehiculosExcel() => File(await _exportService.ExportarVentasVehiculosExcelAsync(), TipoExcel, $"VentasVehiculos_{DateTime.Now:yyyyMMdd}.xlsx");

    [HttpGet("ventas-vehiculos/pdf")]
    public async Task<IActionResult> VentasVehiculosPdf() => File(await _exportService.ExportarVentasVehiculosPdfAsync(), TipoPdf, $"VentasVehiculos_{DateTime.Now:yyyyMMdd}.pdf");

    [HttpGet("rentas/excel")]
    public async Task<IActionResult> RentasExcel() => File(await _exportService.ExportarRentasExcelAsync(), TipoExcel, $"Rentas_{DateTime.Now:yyyyMMdd}.xlsx");

    [HttpGet("rentas/pdf")]
    public async Task<IActionResult> RentasPdf() => File(await _exportService.ExportarRentasPdfAsync(), TipoPdf, $"Rentas_{DateTime.Now:yyyyMMdd}.pdf");
}
