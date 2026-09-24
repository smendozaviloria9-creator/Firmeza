using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Controllers;


[Authorize(Roles = Roles.Administrador)]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            TotalProductos = await _context.Productos.CountAsync(),
            TotalClientes = await _context.Clientes.CountAsync(),
            TotalVentas = await _context.Ventas.CountAsync(),
            MontoTotalVentas = await _context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0m
        };

        return View(vm);
    }

    public IActionResult Error() => View();
}

public class DashboardViewModel
{
    public int TotalProductos { get; set; }
    public int TotalClientes { get; set; }
    public int TotalVentas { get; set; }
    public decimal MontoTotalVentas { get; set; }
}
