using AutoMapper;
using Firmeza.Application.DTOs.VentasVehiculos;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/ventas-vehiculos")]
[Authorize]
public class VentasVehiculosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public VentasVehiculosController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaVehiculoDto>>> GetAll()
    {
        var ventas = await _context.VentasVehiculos
            .Include(v => v.Cliente)
            .Include(v => v.Vehiculo)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<VentaVehiculoDto>>(ventas));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VentaVehiculoDto>> GetById(int id)
    {
        var venta = await _context.VentasVehiculos
            .Include(v => v.Cliente)
            .Include(v => v.Vehiculo)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta is null) return NotFound();
        return Ok(_mapper.Map<VentaVehiculoDto>(venta));
    }

    [HttpPost]
    public async Task<ActionResult<VentaVehiculoDto>> Create(VentaVehiculoCreateDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente is null)
        {
            return BadRequest(new { mensaje = "El cliente seleccionado no existe." });
        }

        var vehiculo = await _context.Vehiculos.FindAsync(dto.VehiculoId);
        if (vehiculo is null)
        {
            return BadRequest(new { mensaje = "El vehiculo seleccionado no existe." });
        }

        if (vehiculo.Estado != "Disponible")
        {
            return BadRequest(new { mensaje = $"El vehiculo no esta disponible para la venta. Estado actual: {vehiculo.Estado}." });
        }

        var subtotal = vehiculo.PrecioVenta;
        var iva = Math.Round(subtotal * ReciboService.PorcentajeIva, 2);

        var venta = new VentaVehiculo
        {
            ClienteId = dto.ClienteId,
            VehiculoId = dto.VehiculoId,
            Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
            Subtotal = subtotal,
            Iva = iva,
            Total = subtotal + iva
        };

        vehiculo.Estado = "Vendido";

        _context.VentasVehiculos.Add(venta);
        await _context.SaveChangesAsync();

        var ventaCompleta = await _context.VentasVehiculos
            .Include(v => v.Cliente)
            .Include(v => v.Vehiculo)
            .FirstAsync(v => v.Id == venta.Id);

        var resultado = _mapper.Map<VentaVehiculoDto>(ventaCompleta);
        return CreatedAtAction(nameof(GetById), new { id = venta.Id }, resultado);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var venta = await _context.VentasVehiculos.FindAsync(id);
        if (venta is null) return NotFound();

        var vehiculo = await _context.Vehiculos.FindAsync(venta.VehiculoId);
        if (vehiculo is not null)
        {
            vehiculo.Estado = "Disponible";
        }

        _context.VentasVehiculos.Remove(venta);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
