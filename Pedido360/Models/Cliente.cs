using System.ComponentModel.DataAnnotations;

namespace Pedido360.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "La cedula o juridica es requerida.")]
    [StringLength(20, ErrorMessage = "La cedula no puede superar 20 caracteres.")]
    [Display(Name = "Cedula / Juridica")]
    public string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "El correo es requerido.")]
    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Ingrese un correo valido.")]
    [Display(Name = "Correo electronico")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "El telefono es requerido.")]
    [StringLength(20, ErrorMessage = "El telefono no puede superar 20 caracteres.")]
    [Display(Name = "Telefono")]
    public string Telefono { get; set; } = null!;

    [Required(ErrorMessage = "La direccion es requerida.")]
    [StringLength(250, ErrorMessage = "La direccion no puede superar 250 caracteres.")]
    [Display(Name = "Direccion")]
    public string Direccion { get; set; } = null!;

    [Display(Name = "Fecha de registro")]
    public DateTime FechaDeRegistro { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Navegacion: un cliente puede tener muchos pedidos
    public virtual ICollection<Pedido>? Pedidos { get; set; }
}
