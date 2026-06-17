using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedido360.Models
{
    public class PedidoDetalle
    {
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }
        public virtual Pedido? Pedido { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public virtual Producto? Producto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }

        public decimal ImpuestoPorc { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalLinea { get; set; }
    }
}
