using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Firmeza.Web.Utils;
using Firmeza.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class ClientesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClientesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Clientes?buscar=...
    public async Task<IActionResult> Index(string? buscar)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(c =>
                c.Nombres.Contains(buscar) ||
                c.Apellidos.Contains(buscar) ||
                c.Documento.Contains(buscar));
        }

        ViewBag.Buscar = buscar;

        var clientes = await query.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).ToListAsync();
        return View(clientes);
    }

    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    public IActionResult Create() => View(new ClienteViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteViewModel vm)
    {
        // Validación manual de la edad con try/catch (Task 7).
        if (!EdadValidator.TryParseEdad(vm.EdadTexto, out var edad, out var errorEdad))
        {
            ModelState.AddModelError(nameof(vm.EdadTexto), errorEdad!);
        }

        if (!await DocumentoYCorreoDisponiblesAsync(vm.Documento, vm.Correo, idExcluido: null))
        {
            ModelState.AddModelError(string.Empty, "Ya existe un cliente registrado con ese documento o correo.");
        }

        if (!ModelState.IsValid) return View(vm);

        var cliente = new Cliente
        {
            Nombres = vm.Nombres,
            Apellidos = vm.Apellidos,
            Documento = vm.Documento,
            Correo = vm.Correo,
            Telefono = vm.Telefono,
            Direccion = vm.Direccion,
            Edad = edad
        };

        _context.Add(cliente);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        var vm = new ClienteViewModel
        {
            Id = cliente.Id,
            Nombres = cliente.Nombres,
            Apellidos = cliente.Apellidos,
            Documento = cliente.Documento,
            Correo = cliente.Correo,
            Telefono = cliente.Telefono,
            Direccion = cliente.Direccion,
            EdadTexto = cliente.Edad.ToString()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (!EdadValidator.TryParseEdad(vm.EdadTexto, out var edad, out var errorEdad))
        {
            ModelState.AddModelError(nameof(vm.EdadTexto), errorEdad!);
        }

        if (!await DocumentoYCorreoDisponiblesAsync(vm.Documento, vm.Correo, idExcluido: id))
        {
            ModelState.AddModelError(string.Empty, "Ya existe otro cliente registrado con ese documento o correo.");
        }

        if (!ModelState.IsValid) return View(vm);

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        cliente.Nombres = vm.Nombres;
        cliente.Apellidos = vm.Apellidos;
        cliente.Documento = vm.Documento;
        cliente.Correo = vm.Correo;
        cliente.Telefono = vm.Telefono;
        cliente.Direccion = vm.Direccion;
        cliente.Edad = edad;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Clientes.AnyAsync(c => c.Id == id)) return NotFound();
            throw;
        }

        TempData["Mensaje"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is not null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Cliente eliminado.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> DocumentoYCorreoDisponiblesAsync(string documento, string correo, int? idExcluido)
    {
        var query = _context.Clientes.Where(c => c.Documento == documento || c.Correo == correo);
        if (idExcluido.HasValue)
        {
            query = query.Where(c => c.Id != idExcluido.Value);
        }
        return !await query.AnyAsync();
    }
}
