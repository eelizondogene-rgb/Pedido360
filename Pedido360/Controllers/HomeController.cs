using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedido360.Data;
using Pedido360.Models;
using Pedido360.ViewModels;

namespace Pedido360.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var inicioDeMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var model = new PanelViewModel
        {
            TotalClientes = await _context.Clientes.CountAsync(c => c.Activo),
            TotalProductos = await _context.Productos.CountAsync(p => p.Activo),
            ProductosBajoStock = await _context.Productos.CountAsync(p => p.Activo && p.Stock <= 5),
            PedidosPendientes = await _context.Pedidos.CountAsync(p => p.Estado == "Pendiente"),
            MontoConfirmadoMes = await _context.Pedidos
                .Where(p => p.Estado == "Confirmado" && p.Fecha >= inicioDeMes)
                .Select(p => (decimal?)p.Total)
                .SumAsync() ?? 0m,
            UltimosPedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.Fecha)
                .Take(6)
                .Select(p => new PedidoResumenViewModel
                {
                    Id = p.Id,
                    ClienteNombre = p.Cliente != null ? p.Cliente.Nombre : "(sin cliente)",
                    Fecha = p.Fecha,
                    Total = p.Total,
                    Estado = p.Estado
                })
                .ToListAsync()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Paginas de error personalizadas para codigos HTTP (404, 500, etc.)
    // Enganchada desde Program.cs via UseStatusCodePagesWithReExecute("/Error/{0}")
    [AllowAnonymous]
    [Route("Error/{statusCode}")]
    public IActionResult StatusCodeHandler(int statusCode)
    {
        Response.StatusCode = statusCode;

        return statusCode switch
        {
            404 => View("NotFound404"),
            403 => View("Forbidden403"),
            _ => View("ServerError500")
        };
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
