using Microsoft.AspNetCore.Mvc.Rendering;
using Pedido360.Models;

namespace Pedido360.ViewModels;

public class ProductoListaViewModel
{
    public List<Producto> Productos { get; set; } = new();
    public IEnumerable<SelectListItem> Categorias { get; set; } = Enumerable.Empty<SelectListItem>();

    public string? Buscar { get; set; }
    public int? CategoriaId { get; set; }

    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public const int TamanoPagina = 8;
}
