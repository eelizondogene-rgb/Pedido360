using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedido360.Data;
using Pedido360.Models;
using Pedido360.ViewModels;

namespace Pedido360.Controllers;

// TODO (Hito 2 - seguridad): agregar [Authorize] a nivel de clase y restringir
// Delete solo a Admin cuando Identity + Roles esten aplicados.
public class ClientesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClientesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Clientes
    // Soporta busqueda por nombre o cedula via parametro "buscar"
    public async Task<IActionResult> Index(string? buscar)
    {
        ViewBag.Buscar = buscar;

        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim().ToLower();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(termino) ||
                c.Cedula.ToLower().Contains(termino));
        }

        var model = await query
            .OrderBy(c => c.Nombre)
            .Select(c => new ClienteListItemViewModel { Cliente = c })
            .ToListAsync();

        return View(model);
    }

    // GET: Clientes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null)
            return NotFound();

        return View(new ClienteDetailsViewModel { Cliente = cliente });
    }

    // GET: Clientes/Create
    public IActionResult Create()
    {
        return View(new ClienteFormViewModel
        {
            Cliente = new Cliente
            {
                FechaDeRegistro = DateTime.Now,
                Activo = true
            }
        });
    }

    // POST: Clientes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteFormViewModel viewModel)
    {
        // Verificar cedula duplicada
        if (await _context.Clientes.AnyAsync(c => c.Cedula == viewModel.Cliente.Cedula))
        {
            ModelState.AddModelError("Cliente.Cedula", "Ya existe un cliente con esa cedula.");
        }

        if (!ModelState.IsValid)
            return View(viewModel);

        viewModel.Cliente.FechaDeRegistro = DateTime.Now;
        _context.Add(viewModel.Cliente);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cliente registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Clientes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound();

        return View(new ClienteFormViewModel { Cliente = cliente });
    }

    // POST: Clientes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteFormViewModel viewModel)
    {
        if (id != viewModel.Cliente.Id)
            return NotFound();

        // Verificar cedula duplicada en otro cliente
        if (await _context.Clientes.AnyAsync(c =>
                c.Cedula == viewModel.Cliente.Cedula && c.Id != id))
        {
            ModelState.AddModelError("Cliente.Cedula", "Ya existe otro cliente con esa cedula.");
        }

        if (!ModelState.IsValid)
            return View(viewModel);

        var clienteDb = await _context.Clientes.FindAsync(id);
        if (clienteDb is null)
            return NotFound();

        clienteDb.Nombre    = viewModel.Cliente.Nombre;
        clienteDb.Cedula    = viewModel.Cliente.Cedula;
        clienteDb.Correo    = viewModel.Cliente.Correo;
        clienteDb.Telefono  = viewModel.Cliente.Telefono;
        clienteDb.Direccion = viewModel.Cliente.Direccion;
        clienteDb.Activo    = viewModel.Cliente.Activo;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Clientes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null)
            return NotFound();

        return View(new ClienteDetailsViewModel { Cliente = cliente });
    }

    // POST: Clientes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is not null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cliente eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
