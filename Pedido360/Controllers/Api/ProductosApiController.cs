using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedido360.Data;
using Pedido360.ViewModels.Api;

namespace Pedido360.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/productos")]
public class ProductosApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductosApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/productos/buscar?q=arr
    // Autosuggest usado por el constructor de pedidos: hasta 10 coincidencias
    // por nombre, solo productos activos con stock disponible.
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<ProductoBusquedaResultado>>> Buscar([FromQuery] string? q)
    {
        var termino = (q ?? string.Empty).Trim().ToLower();

        var query = _context.Productos
            .Where(p => p.Activo && p.Stock > 0)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(termino))
        {
            query = query.Where(p => p.Nombre.ToLower().Contains(termino));
        }

        var resultados = await query
            .OrderBy(p => p.Nombre)
            .Take(10)
            .Select(p => new ProductoBusquedaResultado
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Impuesto = p.ImpuestoPorc,
                Stock = p.Stock
            })
            .ToListAsync();

        return Ok(resultados);
    }
}
