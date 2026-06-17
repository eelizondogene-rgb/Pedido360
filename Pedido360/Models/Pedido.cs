using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Pedido360.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }
        public virtual Cliente? Cliente { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        [ForeignKey("UsuarioId")]
        public virtual IdentityUser? Usuario { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuestos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";

        public virtual ICollection<PedidoDetalle>? Detalles { get; set; }
    }
}
