using AutoMapper;
using Firmeza.Application.DTOs.Vehiculos;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiculosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public VehiculosController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehiculoDto>>> GetAll([FromQuery] string? buscar, [FromQuery] string? tipo, [FromQuery] string? estado)
    {
        var query = _context.Vehiculos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(v => v.Marca.Contains(buscar) || v.Modelo.Contains(buscar) || v.Placa.Contains(buscar));
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            query = query.Where(v => v.Tipo == tipo);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(v => v.Estado == estado);
        }

        var vehiculos = await query.OrderBy(v => v.Marca).ThenBy(v => v.Modelo).ToListAsync();
        return Ok(_mapper.Map<IEnumerable<VehiculoDto>>(vehiculos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehiculoDto>> GetById(int id)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo is null) return NotFound();
        return Ok(_mapper.Map<VehiculoDto>(vehiculo));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<VehiculoDto>> Create(VehiculoCreateDto dto)
    {
        if (await _context.Vehiculos.AnyAsync(v => v.Placa == dto.Placa))
        {
            return BadRequest(new { mensaje = "Ya existe un vehiculo registrado con esa placa." });
        }

        var vehiculo = _mapper.Map<Vehiculo>(dto);
        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        var resultado = _mapper.Map<VehiculoDto>(vehiculo);
        return CreatedAtAction(nameof(GetById), new { id = vehiculo.Id }, resultado);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, VehiculoUpdateDto dto)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo is null) return NotFound();

        if (await _context.Vehiculos.AnyAsync(v => v.Id != id && v.Placa == dto.Placa))
        {
            return BadRequest(new { mensaje = "Ya existe otro vehiculo registrado con esa placa." });
        }

        _mapper.Map(dto, vehiculo);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo is null) return NotFound();

        var tieneMovimientos = await _context.VentasVehiculos.AnyAsync(vv => vv.VehiculoId == id)
            || await _context.Rentas.AnyAsync(r => r.VehiculoId == id);

        if (tieneMovimientos)
        {
            return BadRequest(new { mensaje = "No se puede eliminar: el vehiculo tiene ventas o rentas asociadas." });
        }

        _context.Vehiculos.Remove(vehiculo);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
