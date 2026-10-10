using AutoMapper;
using Firmeza.Application.DTOs.Rentas;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RentasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RentasController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RentaDto>>> GetAll()
    {
        var rentas = await _context.Rentas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo)
            .OrderByDescending(r => r.FechaInicio)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<RentaDto>>(rentas));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RentaDto>> GetById(int id)
    {
        var renta = await _context.Rentas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (renta is null) return NotFound();
        return Ok(_mapper.Map<RentaDto>(renta));
    }

    [HttpPost]
    public async Task<ActionResult<RentaDto>> Create(RentaCreateDto dto)
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

        if (dto.FechaFin <= dto.FechaInicio)
        {
            return BadRequest(new { mensaje = "La fecha de fin debe ser posterior a la fecha de inicio." });
        }

        if (vehiculo.Estado == "Vendido")
        {
            return BadRequest(new { mensaje = "El vehiculo ya fue vendido y no se puede rentar." });
        }

        var inicio = DateTime.SpecifyKind(dto.FechaInicio, DateTimeKind.Utc);
        var fin = DateTime.SpecifyKind(dto.FechaFin, DateTimeKind.Utc);

        var hayCruce = await _context.Rentas.AnyAsync(r =>
            r.VehiculoId == dto.VehiculoId &&
            r.Estado == "Activa" &&
            r.FechaInicio < fin &&
            r.FechaFin > inicio);

        if (hayCruce)
        {
            return BadRequest(new { mensaje = "El vehiculo ya tiene una renta activa que se cruza con esas fechas." });
        }

        var dias = (int)Math.Ceiling((fin - inicio).TotalDays);

        var renta = new Renta
        {
            ClienteId = dto.ClienteId,
            VehiculoId = dto.VehiculoId,
            FechaInicio = inicio,
            FechaFin = fin,
            TarifaDia = vehiculo.PrecioRentaDia,
            Total = dias * vehiculo.PrecioRentaDia,
            Estado = "Activa"
        };

        vehiculo.Estado = "Rentado";

        _context.Rentas.Add(renta);
        await _context.SaveChangesAsync();

        var rentaCompleta = await _context.Rentas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo)
            .FirstAsync(r => r.Id == renta.Id);

        var resultado = _mapper.Map<RentaDto>(rentaCompleta);
        return CreatedAtAction(nameof(GetById), new { id = renta.Id }, resultado);
    }

    [HttpPut("{id}/finalizar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Finalizar(int id)
    {
        var renta = await _context.Rentas.FindAsync(id);
        if (renta is null) return NotFound();

        if (renta.Estado != "Activa")
        {
            return BadRequest(new { mensaje = "Solo se puede finalizar una renta activa." });
        }

        renta.Estado = "Finalizada";
        await LiberarVehiculoSiCorrespondeAsync(renta.VehiculoId, renta.Id);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var renta = await _context.Rentas.FindAsync(id);
        if (renta is null) return NotFound();

        var vehiculoId = renta.VehiculoId;
        _context.Rentas.Remove(renta);
        await LiberarVehiculoSiCorrespondeAsync(vehiculoId, id);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task LiberarVehiculoSiCorrespondeAsync(int vehiculoId, int rentaIdExcluida)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(vehiculoId);
        if (vehiculo is null || vehiculo.Estado != "Rentado") return;

        var otraActiva = await _context.Rentas.AnyAsync(r =>
            r.VehiculoId == vehiculoId &&
            r.Id != rentaIdExcluida &&
            r.Estado == "Activa");

        if (!otraActiva)
        {
            vehiculo.Estado = "Disponible";
        }
    }
}
