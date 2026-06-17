using System.ComponentModel.DataAnnotations;

namespace Pedido360.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public virtual ICollection<Producto>? Productos { get; set; }
    }
}
