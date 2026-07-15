namespace Pedido360.ViewModels.Api;

public class ProductoBusquedaResultado
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal Impuesto { get; set; }
    public int Stock { get; set; }
}

public class CalcularPedidoRequest
{
    public List<CalcularPedidoLinea> Lineas { get; set; } = new();
}

public class CalcularPedidoLinea
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }

    /// <summary>Descuento fijo en colones aplicado sobre el total de la linea.</summary>
    public decimal Descuento { get; set; }
}

public class CalcularPedidoLineaResultado
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnit { get; set; }
    public decimal Descuento { get; set; }
    public decimal ImpuestoPorc { get; set; }
    public decimal ImpuestoMonto { get; set; }
    public decimal TotalLinea { get; set; }
    public bool StockSuficiente { get; set; }
    public int StockDisponible { get; set; }
}

public class CalcularPedidoResponse
{
    public List<CalcularPedidoLineaResultado> Lineas { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Descuentos { get; set; }
    public decimal Total { get; set; }
    public bool Valido { get; set; } = true;
    public List<string> Errores { get; set; } = new();
}
