using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Pedido360.ViewModels;

public class PedidoFormViewModel
{
    [Required(ErrorMessage = "Seleccione un cliente.")]
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }

    public List<PedidoLineaFormViewModel> Lineas { get; set; } = new();

    public IEnumerable<SelectListItem> Clientes { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class PedidoLineaFormViewModel
{
    [Required(ErrorMessage = "Seleccione un producto.")]
    public int ProductoId { get; set; }

    // Se muestra en la fila para no tener que volver a buscarlo; no se
    // valida en servidor, solo es referencia visual.
    public string? ProductoNombre { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Cantidad { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo.")]
    public decimal Descuento { get; set; } = 0;
}
