using AutoMapper;
using Firmeza.Application.DTOs.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IReciboService _reciboService;
    private readonly IWebHostEnvironment _env;

    public VentasController(ApplicationDbContext context, IMapper mapper, IReciboService reciboService, IWebHostEnvironment env)
    {
        _context = context;
        _mapper = mapper;
        _reciboService = reciboService;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaDto>>> GetAll()
    {
        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<VentaDto>>(ventas));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VentaDto>> GetById(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta is null) return NotFound();
        return Ok(_mapper.Map<VentaDto>(venta));
    }

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Create(VentaCreateDto dto)
    {
        var productoIds = dto.Items.Select(i => i.ProductoId).Distinct().ToList();
        var productos = await _context.Productos.Where(p => productoIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        var cantidadesPorProducto = dto.Items.GroupBy(i => i.ProductoId).ToDictionary(g => g.Key, g => g.Sum(i => i.Cantidad));

        foreach (var kvp in cantidadesPorProducto)
        {
            if (!productos.TryGetValue(kvp.Key, out var producto))
            {
                return BadRequest(new { mensaje = $"El producto Id {kvp.Key} no existe." });
            }

            if (kvp.Value > producto.Stock)
            {
                return BadRequest(new { mensaje = $"No hay stock suficiente de {producto.Nombre}. Solicitado: {kvp.Value}, disponible: {producto.Stock}." });
            }
        }

        var venta = new Venta
        {
            ClienteId = dto.ClienteId,
            Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
            Detalles = new List<DetalleVenta>()
        };

        decimal subtotalGeneral = 0;

        foreach (var item in dto.Items)
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

        var ventaCompleta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstAsync(v => v.Id == venta.Id);

        var carpetaRecibos = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "recibos");
        if (!Directory.Exists(carpetaRecibos)) Directory.CreateDirectory(carpetaRecibos);

        var nombreArchivo = _reciboService.GenerarRecibo(ventaCompleta, carpetaRecibos);
        venta.ReciboArchivo = nombreArchivo;
        await _context.SaveChangesAsync();

        var resultado = _mapper.Map<VentaDto>(ventaCompleta);
        return CreatedAtAction(nameof(GetById), new { id = venta.Id }, resultado);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);
        if (venta is null) return NotFound();

        _context.Ventas.Remove(venta);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
