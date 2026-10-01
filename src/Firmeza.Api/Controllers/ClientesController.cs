using AutoMapper;
using Firmeza.Application.DTOs.Clientes;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ClientesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ClientesController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll([FromQuery] string? buscar)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(c => c.Nombres.Contains(buscar) || c.Apellidos.Contains(buscar) || c.Documento.Contains(buscar));
        }

        var clientes = await query.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).ToListAsync();
        return Ok(_mapper.Map<IEnumerable<ClienteDto>>(clientes));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();
        return Ok(_mapper.Map<ClienteDto>(cliente));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create(ClienteCreateDto dto)
    {
        var existe = await _context.Clientes.AnyAsync(c => c.Documento == dto.Documento || c.Correo == dto.Correo);
        if (existe)
        {
            return BadRequest(new { mensaje = "Ya existe un cliente registrado con ese documento o correo." });
        }

        var cliente = _mapper.Map<Cliente>(dto);
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        var resultado = _mapper.Map<ClienteDto>(cliente);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, resultado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ClienteUpdateDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        var duplicado = await _context.Clientes.AnyAsync(c => c.Id != id && (c.Documento == dto.Documento || c.Correo == dto.Correo));
        if (duplicado)
        {
            return BadRequest(new { mensaje = "Ya existe otro cliente registrado con ese documento o correo." });
        }

        _mapper.Map(dto, cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
