namespace Pedido360.Services;

public readonly record struct LineaCalculada(
    decimal SubtotalBruto,
    decimal SubtotalConDescuento,
    decimal ImpuestoMonto,
    decimal TotalLinea);

/// <summary>
/// Formula unica para calcular el total de una linea de pedido. La usan tanto
/// el endpoint AJAX (POST /api/pedidos/calcular) como el controller de
/// Pedidos al guardar, para que el numero que ve el usuario en vivo sea
/// exactamente el mismo que se persiste.
///
/// El descuento se maneja como un monto fijo en colones por linea (coincide
/// con PedidoDetalle.Descuento, que ya existia en el modelo de datos).
/// </summary>
public static class PedidoMath
{
    public static LineaCalculada CalcularLinea(decimal precioUnit, int cantidad, decimal descuentoMonto, decimal impuestoPorc)
    {
        var subtotalBruto = precioUnit * cantidad;

        if (descuentoMonto < 0) descuentoMonto = 0;
        if (descuentoMonto > subtotalBruto) descuentoMonto = subtotalBruto;

        var subtotalConDescuento = subtotalBruto - descuentoMonto;
        var impuestoMonto = Math.Round(subtotalConDescuento * (impuestoPorc / 100m), 2);
        var totalLinea = subtotalConDescuento + impuestoMonto;

        return new LineaCalculada(subtotalBruto, subtotalConDescuento, impuestoMonto, totalLinea);
    }
}
