using AutoMapper;
using Firmeza.Application.DTOs.Productos;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProductosController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll([FromQuery] string? buscar, [FromQuery] string? categoria)
    {
        var query = _context.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(p => p.Nombre.Contains(buscar) || (p.Descripcion != null && p.Descripcion.Contains(buscar)));
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            query = query.Where(p => p.Categoria == categoria);
        }

        var productos = await query.OrderBy(p => p.Nombre).ToListAsync();
        return Ok(_mapper.Map<IEnumerable<ProductoDto>>(productos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoDto>> GetById(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound();
        return Ok(_mapper.Map<ProductoDto>(producto));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProductoDto>> Create(ProductoCreateDto dto)
    {
        var producto = _mapper.Map<Producto>(dto);
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        var resultado = _mapper.Map<ProductoDto>(producto);
        return CreatedAtAction(nameof(GetById), new { id = producto.Id }, resultado);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, ProductoUpdateDto dto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound();

        _mapper.Map(dto, producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound();

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
