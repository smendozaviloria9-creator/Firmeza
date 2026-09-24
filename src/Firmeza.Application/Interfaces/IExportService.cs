namespace Firmeza.Application.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportarProductosExcelAsync();
    Task<byte[]> ExportarProductosPdfAsync();
    Task<byte[]> ExportarClientesExcelAsync();
    Task<byte[]> ExportarClientesPdfAsync();
    Task<byte[]> ExportarVentasExcelAsync();
    Task<byte[]> ExportarVentasPdfAsync();
}
