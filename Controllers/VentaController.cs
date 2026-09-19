using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Firmeza.Web.Services;
using Firmeza.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class VentasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IReciboService _reciboService;
    private readonly IWebHostEnvironment _env;

    public VentasController(ApplicationDbContext context, IReciboService reciboService, IWebHostEnvironment env)
    {
        _context = context;
        _reciboService = reciboService;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
        return View(ventas);
    }

    public async Task<IActionResult> Details(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta is null) return NotFound();
        return View(venta);
    }

    public async Task<IActionResult> Create()
    {
        await CargarListasAsync();
        return View(new VentaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VentaViewModel vm)
    {
        if (vm.Items == null || vm.Items.Count == 0)
        {
            ModelState.AddModelError("", "Debes agregar al menos un producto.");
        }

        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(vm);
        }

        // Cargar productos involucrados para tomar su precio actual
        var productoIds = vm.Items.Select(i => i.ProductoId).ToList();
        var productos = await _context.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // Validar existencia y stock disponible antes de guardar nada
        foreach (var item in vm.Items)
        {
            if (!productos.TryGetValue(item.ProductoId, out var productoValidar))
            {
                ModelState.AddModelError("", $"El producto seleccionado (Id {item.ProductoId}) no existe.");
                continue;
            }

            if (item.Cantidad > productoValidar.Stock)
            {
                ModelState.AddModelError("",
                    $"No hay stock suficiente de '{productoValidar.Nombre}'. Disponible: {productoValidar.Stock}.");
            }
        }

        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(vm);
        }

        var venta = new Venta
        {
            ClienteId = vm.ClienteId,
            Fecha = DateTime.SpecifyKind(vm.Fecha, DateTimeKind.Utc),
            Detalles = new List<DetalleVenta>()
        };

        decimal subtotalGeneral = 0;

        foreach (var item in vm.Items)
        {
            var producto = productos[item.ProductoId];

            var subtotalLinea = item.Cantidad * producto.PrecioUnitario;
            subtotalGeneral += subtotalLinea;

            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.PrecioUnitario,
                Subtotal = subtotalLinea
            });

            producto.Stock -= item.Cantidad;
        }

        venta.Subtotal = subtotalGeneral;
        venta.Iva = Math.Round(subtotalGeneral * ReciboService.PorcentajeIva, 2);
        venta.Total = venta.Subtotal + venta.Iva;

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        // Recargar con las relaciones necesarias para el PDF
        var ventaCompleta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstAsync(v => v.Id == venta.Id);

        var carpetaRecibos = Path.Combine(_env.WebRootPath, "recibos");
        var nombreArchivo = _reciboService.GenerarRecibo(ventaCompleta, carpetaRecibos);

        venta.ReciboArchivo = nombreArchivo;
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Venta registrada correctamente. Recibo generado.";
        return RedirectToAction(nameof(Details), new { id = venta.Id });
    }

    public IActionResult DescargarRecibo(int id)
    {
        var venta = _context.Ventas.Find(id);
        if (venta is null || string.IsNullOrEmpty(venta.ReciboArchivo))
            return NotFound();

        var ruta = Path.Combine(_env.WebRootPath, "recibos", venta.ReciboArchivo);
        if (!System.IO.File.Exists(ruta))
            return NotFound();

        return PhysicalFile(ruta, "application/pdf", venta.ReciboArchivo);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var venta = await _context.Ventas.Include(v => v.Cliente).FirstOrDefaultAsync(v => v.Id == id);
        if (venta is null) return NotFound();
        return View(venta);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);
        if (venta is not null)
        {
            if (!string.IsNullOrEmpty(venta.ReciboArchivo))
            {
                var ruta = Path.Combine(_env.WebRootPath, "recibos", venta.ReciboArchivo);
                if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
            }

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Venta eliminada.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Clientes = await _context.Clientes
            .OrderBy(c => c.Nombres)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombres + " " + c.Apellidos
            })
            .ToListAsync();

        ViewBag.Productos = await _context.Productos
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }
}