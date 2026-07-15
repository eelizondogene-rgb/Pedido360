using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedido360.Data;
using Pedido360.Models;
using Pedido360.ViewModels;

namespace Pedido360.Controllers;

[Authorize]
public class ProductosController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Productos
    // Soporta busqueda por nombre, filtro por categoria y paginacion.
    public async Task<IActionResult> Index(string? buscar, int? categoriaId, int pagina = 1)
    {
        var query = _context.Productos
            .Include(p => p.Categoria)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(termino));
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        var totalRegistros = await query.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)ProductoListaViewModel.TamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var productos = await query
            .OrderBy(p => p.Nombre)
            .Skip((pagina - 1) * ProductoListaViewModel.TamanoPagina)
            .Take(ProductoListaViewModel.TamanoPagina)
            .ToListAsync();

        var model = new ProductoListaViewModel
        {
            Productos = productos,
            Categorias = await ObtenerCategoriasSelectAsync(categoriaId),
            Buscar = buscar,
            CategoriaId = categoriaId,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = totalRegistros
        };

        return View(model);
    }

    // GET: Productos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto is null)
            return NotFound();

        return View(producto);
    }

    // GET: Productos/Create
    [Authorize(Roles = "Admin,Operaciones")]
    public IActionResult Create()
    {
        CargarCategorias();
        return View(new Producto
        {
            Activo = true
        });
    }

    // POST: Productos/Create
    // La imagen es obligatoria al crear (criterio de aceptacion H2.1).
    [HttpPost]
    [Authorize(Roles = "Admin,Operaciones")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (string.IsNullOrWhiteSpace(producto.ImagenUrl))
        {
            ModelState.AddModelError(nameof(producto.ImagenUrl), "La imagen es obligatoria para publicar el producto.");
        }

        if (!ModelState.IsValid)
        {
            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Producto registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Productos/Edit/5
    [Authorize(Roles = "Admin,Operaciones")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var producto = await _context.Productos.FindAsync(id);
        if (producto is null)
            return NotFound();

        CargarCategorias(producto.CategoriaId);
        return View(producto);
    }

    // POST: Productos/Edit/5
    // Al editar, la imagen puede dejarse en blanco: se conserva la anterior
    // (criterio de aceptacion H2.3), a diferencia de la creacion.
    [HttpPost]
    [Authorize(Roles = "Admin,Operaciones")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id)
            return NotFound();

        ModelState.Remove(nameof(producto.ImagenUrl));

        if (!ModelState.IsValid)
        {
            CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        var productoDb = await _context.Productos.FindAsync(id);
        if (productoDb is null)
            return NotFound();

        productoDb.Nombre = producto.Nombre;
        productoDb.CategoriaId = producto.CategoriaId;
        productoDb.Precio = producto.Precio;
        productoDb.ImpuestoPorc = producto.ImpuestoPorc;
        productoDb.Stock = producto.Stock;
        if (!string.IsNullOrWhiteSpace(producto.ImagenUrl))
        {
            productoDb.ImagenUrl = producto.ImagenUrl;
        }
        productoDb.Activo = producto.Activo;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Productos/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto is null)
            return NotFound();

        return View(producto);
    }

    // POST: Productos/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _context.Productos.FindAsync(id);

        if (producto is not null)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Producto eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void CargarCategorias(int? categoriaId = null)
    {
        ViewBag.Categorias = new SelectList(
            _context.Categorias.OrderBy(c => c.Nombre),
            "Id",
            "Nombre",
            categoriaId
        );
    }

    private async Task<IEnumerable<SelectListItem>> ObtenerCategoriasSelectAsync(int? categoriaId)
    {
        var categorias = await _context.Categorias
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombre,
                Selected = categoriaId.HasValue && c.Id == categoriaId.Value
            })
            .ToListAsync();

        return categorias;
    }
}
