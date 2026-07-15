namespace Pedido360.ViewModels;

public class PanelViewModel
{
    public int TotalClientes { get; set; }
    public int TotalProductos { get; set; }
    public int ProductosBajoStock { get; set; }
    public int PedidosPendientes { get; set; }
    public decimal MontoConfirmadoMes { get; set; }
    public List<PedidoResumenViewModel> UltimosPedidos { get; set; } = new();
}

public class PedidoResumenViewModel
{
    public int Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
}
