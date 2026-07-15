using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedido360.Data;
using Pedido360.Models;
using Pedido360.Services;
using Pedido360.ViewModels;

namespace Pedido360.Controllers;

// Admin, Ventas y Operaciones pueden ver pedidos. Crear/confirmar/cancelar
// queda restringido a quien vende (ver overrides por accion mas abajo).
[Authorize]
public class PedidosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PedidosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Pedidos
    // Soporta filtro por estado (Pendiente / Confirmado / Cancelado)
    public async Task<IActionResult> Index(string? estado)
    {
        ViewBag.Estado = estado;

        var query = _context.Pedidos
            .Include(p => p.Cliente)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(p => p.Estado == estado);
        }

        var pedidos = await query
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();

        return View(pedidos);
    }

    // GET: Pedidos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var pedido = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles!)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return NotFound();

        return View(pedido);
    }

    // GET: Pedidos/Create
    [Authorize(Roles = "Admin,Ventas")]
    public async Task<IActionResult> Create()
    {
        var model = new PedidoFormViewModel();
        await CargarListasAsync(model);
        return View(model);
    }

    // POST: Pedidos/Create
    // Recalcula precios, impuestos y totales en el servidor a partir de los
    // datos vigentes del producto (nunca se confia en el precio del cliente).
    [HttpPost]
    [Authorize(Roles = "Admin,Ventas")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PedidoFormViewModel form)
    {
        form.Lineas = (form.Lineas ?? new())
            .Where(l => l.ProductoId > 0 && l.Cantidad > 0)
            .ToList();

        if (form.Lineas.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Agregue al menos un producto al pedido.");
        }

        var cliente = await _context.Clientes.FindAsync(form.ClienteId);
        if (cliente is null || !cliente.Activo)
        {
            ModelState.AddModelError(nameof(form.ClienteId), "Seleccione un cliente valido y activo.");
        }

        if (!ModelState.IsValid)
        {
            await CargarListasAsync(form);
            return View(form);
        }

        var productoIds = form.Lineas.Select(l => l.ProductoId).Distinct().ToList();
        var productos = await _context.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var pedido = new Pedido
        {
            ClienteId = form.ClienteId,
            UsuarioId = await ObtenerUsuarioIdAsync(),
            Fecha = DateTime.Now,
            Estado = "Pendiente",
            Detalles = new List<PedidoDetalle>()
        };

        decimal subtotal = 0m;
        decimal impuestos = 0m;

        foreach (var linea in form.Lineas)
        {
            if (!productos.TryGetValue(linea.ProductoId, out var producto) || !producto.Activo)
            {
                ModelState.AddModelError(string.Empty, "Uno de los productos seleccionados ya no esta disponible.");
                await CargarListasAsync(form);
                return View(form);
            }

            if (linea.Cantidad > producto.Stock)
            {
                ModelState.AddModelError(string.Empty,
                    $"No hay suficiente stock de \"{producto.Nombre}\" (disponible: {producto.Stock}).");
                await CargarListasAsync(form);
                return View(form);
            }

            var calculo = PedidoMath.CalcularLinea(producto.Precio, linea.Cantidad, linea.Descuento, producto.ImpuestoPorc);

            pedido.Detalles.Add(new PedidoDetalle
            {
                ProductoId = producto.Id,
                Cantidad = linea.Cantidad,
                PrecioUnit = producto.Precio,
                Descuento = calculo.SubtotalBruto - calculo.SubtotalConDescuento,
                ImpuestoPorc = producto.ImpuestoPorc,
                TotalLinea = calculo.TotalLinea
            });

            subtotal += calculo.SubtotalConDescuento;
            impuestos += calculo.ImpuestoMonto;
        }

        pedido.Subtotal = subtotal;
        pedido.Impuestos = impuestos;
        pedido.Total = subtotal + impuestos;

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Pedido #{pedido.Id:D4} registrado como pendiente.";
        return RedirectToAction(nameof(Details), new { id = pedido.Id });
    }

    // POST: Pedidos/Confirmar/5
    // Al confirmar es cuando se descuenta el inventario (no en la creacion).
    [HttpPost]
    [Authorize(Roles = "Admin,Ventas")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Detalles!)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return NotFound();

        if (pedido.Estado != "Pendiente")
        {
            TempData["ErrorMessage"] = "Solo se pueden confirmar pedidos pendientes.";
            return RedirectToAction(nameof(Details), new { id });
        }

        foreach (var detalle in pedido.Detalles!)
        {
            if (detalle.Producto is null || detalle.Producto.Stock < detalle.Cantidad)
            {
                TempData["ErrorMessage"] =
                    $"No se pudo confirmar: stock insuficiente de \"{detalle.Producto?.Nombre}\".";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        foreach (var detalle in pedido.Detalles!)
        {
            detalle.Producto!.Stock -= detalle.Cantidad;
        }

        pedido.Estado = "Confirmado";
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Pedido #{pedido.Id:D4} confirmado. Inventario actualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Pedidos/Cancelar/5
    // Si el pedido ya estaba confirmado, se devuelve el inventario reservado.
    [HttpPost]
    [Authorize(Roles = "Admin,Ventas")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Detalles!)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return NotFound();

        if (pedido.Estado == "Cancelado")
        {
            TempData["ErrorMessage"] = "El pedido ya estaba cancelado.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (pedido.Estado == "Confirmado")
        {
            foreach (var detalle in pedido.Detalles!)
            {
                if (detalle.Producto is not null)
                    detalle.Producto.Stock += detalle.Cantidad;
            }
        }

        pedido.Estado = "Cancelado";
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Pedido #{pedido.Id:D4} cancelado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task CargarListasAsync(PedidoFormViewModel model)
    {
        model.Clientes = await _context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Nombre })
            .ToListAsync();
    }

    private async Task<string> ObtenerUsuarioIdAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var currentId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(currentId))
                return currentId;
        }

        // Resguardo defensivo: con [Authorize] en el controller esto no
        // deberia ocurrir, pero evita romper el guardado si llegara a pasar.
        var admin = await _userManager.FindByEmailAsync("admin@pedido360.com");
        return admin?.Id ?? string.Empty;
    }
}
