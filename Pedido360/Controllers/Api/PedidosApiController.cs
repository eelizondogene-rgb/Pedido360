using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedido360.Data;
using Pedido360.Services;
using Pedido360.ViewModels.Api;

namespace Pedido360.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/pedidos")]
public class PedidosApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PedidosApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST /api/pedidos/calcular
    // Recibe las lineas que el usuario esta armando en la vista de Crear
    // pedido y devuelve subtotal, impuestos, descuentos y total calculados
    // con los precios vigentes en base de datos (nunca con lo que mande el
    // cliente). Tambien informa si el stock alcanza para cada linea.
    [HttpPost("calcular")]
    public async Task<ActionResult<CalcularPedidoResponse>> Calcular([FromBody] CalcularPedidoRequest request)
    {
        var response = new CalcularPedidoResponse();

        var lineasValidas = (request.Lineas ?? new())
            .Where(l => l.ProductoId > 0 && l.Cantidad > 0)
            .ToList();

        if (lineasValidas.Count == 0)
        {
            return Ok(response);
        }

        var productoIds = lineasValidas.Select(l => l.ProductoId).Distinct().ToList();
        var productos = await _context.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var linea in lineasValidas)
        {
            if (!productos.TryGetValue(linea.ProductoId, out var producto) || !producto.Activo)
            {
                response.Errores.Add("Un producto del pedido ya no esta disponible.");
                response.Valido = false;
                continue;
            }

            var calculo = PedidoMath.CalcularLinea(producto.Precio, linea.Cantidad, linea.Descuento, producto.ImpuestoPorc);
            var stockSuficiente = linea.Cantidad <= producto.Stock;

            if (!stockSuficiente)
            {
                response.Errores.Add($"Stock insuficiente de \"{producto.Nombre}\" (disponible: {producto.Stock}).");
                response.Valido = false;
            }

            response.Lineas.Add(new CalcularPedidoLineaResultado
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                Cantidad = linea.Cantidad,
                PrecioUnit = producto.Precio,
                Descuento = calculo.SubtotalBruto - calculo.SubtotalConDescuento,
                ImpuestoPorc = producto.ImpuestoPorc,
                ImpuestoMonto = calculo.ImpuestoMonto,
                TotalLinea = calculo.TotalLinea,
                StockSuficiente = stockSuficiente,
                StockDisponible = producto.Stock
            });

            response.Subtotal += calculo.SubtotalConDescuento;
            response.Impuestos += calculo.ImpuestoMonto;
            response.Descuentos += calculo.SubtotalBruto - calculo.SubtotalConDescuento;
        }

        response.Total = response.Subtotal + response.Impuestos;

        return Ok(response);
    }
}
