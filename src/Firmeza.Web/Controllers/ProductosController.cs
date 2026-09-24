using Firmeza.Infrastructure.Data;
using Firmeza.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Controllers;


[Authorize(Roles = Roles.Administrador)]
public class ProductosController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Productos?buscar=...&categoria=...
    public async Task<IActionResult> Index(string? buscar, string? categoria)
    {
        var query = _context.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(p => p.Nombre.Contains(buscar) || p.Descripcion!.Contains(buscar));
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            query = query.Where(p => p.Categoria == categoria);
        }

        ViewBag.Buscar = buscar;
        ViewBag.Categoria = categoria;
        ViewBag.Categorias = await _context.Productos
            .Select(p => p.Categoria)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var productos = await query.OrderBy(p => p.Nombre).ToListAsync();
        return View(productos);
    }

    public async Task<IActionResult> Details(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound();
        return View(producto);
    }

    public IActionResult Create() => View(new ProductoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductoViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var producto = new Producto
        {
            Nombre = vm.Nombre,
            Descripcion = vm.Descripcion,
            Categoria = vm.Categoria,
            UnidadMedida = vm.UnidadMedida,
            PrecioUnitario = vm.PrecioUnitario,
            Stock = vm.Stock,
            Activo = vm.Activo
        };

        _context.Add(producto);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound();

        var vm = new ProductoViewModel
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Categoria = producto.Categoria,
            UnidadMedida = producto.UnidadMedida,
            PrecioUnitario = producto.PrecioUnitario,
            Stock = producto.Stock,
            Activo = producto.Activo
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductoViewModel vm)
    {
        if (id != vm.Id) return NotFound();
        if (!ModelState.IsValid) return View(vm);

        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound();

        producto.Nombre = vm.Nombre;
        producto.Descripcion = vm.Descripcion;
        producto.Categoria = vm.Categoria;
        producto.UnidadMedida = vm.UnidadMedida;
        producto.PrecioUnitario = vm.PrecioUnitario;
        producto.Stock = vm.Stock;
        producto.Activo = vm.Activo;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Productos.AnyAsync(p => p.Id == id)) return NotFound();
            throw;
        }

        TempData["Mensaje"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound();
        return View(producto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is not null)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Producto eliminado.";
        }

        return RedirectToAction(nameof(Index));
    }
}
