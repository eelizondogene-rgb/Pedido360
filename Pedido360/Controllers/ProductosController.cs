using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedido360.Data;
using Pedido360.Models;

namespace Pedido360.Controllers;

// TODO (Hito 2 - seguridad): agregar [Authorize] a nivel de clase y restringir
// Delete solo a Admin cuando Identity + Roles esten aplicados.
public class ProductosController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Productos
    // Soporta busqueda por nombre
    public async Task<IActionResult> Index(string? buscar)
    {
        ViewBag.Buscar = buscar;

        var query = _context.Productos
            .Include(p => p.Categoria)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(termino));
        }

        var productos = await query
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return View(productos);
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
    public IActionResult Create()
    {
        CargarCategorias();
        return View(new Producto
        {
            Activo = true
        });
    }

    // POST: Productos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id)
            return NotFound();

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
        productoDb.ImagenUrl = producto.ImagenUrl;
        productoDb.Activo = producto.Activo;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Productos/Delete/5
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
}