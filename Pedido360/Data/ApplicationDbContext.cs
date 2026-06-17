using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pedido360.Models;

namespace Pedido360.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoDetalle> PedidoDetalles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Precision configuration for decimal types if needed (already handled via DataAnnotations/Column attribute but good to be explicit)
        builder.Entity<Producto>()
            .Property(p => p.Precio)
            .HasPrecision(18, 2);

        builder.Entity<Producto>()
            .Property(p => p.ImpuestoPorc)
            .HasPrecision(18, 2);

        builder.Entity<Pedido>()
            .Property(p => p.Subtotal)
            .HasPrecision(18, 2);
        
        builder.Entity<Pedido>()
            .Property(p => p.Impuestos)
            .HasPrecision(18, 2);

        builder.Entity<Pedido>()
            .Property(p => p.Total)
            .HasPrecision(18, 2);

        builder.Entity<PedidoDetalle>()
            .Property(p => p.PrecioUnit)
            .HasPrecision(18, 2);

        builder.Entity<PedidoDetalle>()
            .Property(p => p.Descuento)
            .HasPrecision(18, 2);

        builder.Entity<PedidoDetalle>()
            .Property(p => p.ImpuestoPorc)
            .HasPrecision(18, 2);

        builder.Entity<PedidoDetalle>()
            .Property(p => p.TotalLinea)
            .HasPrecision(18, 2);
    }
}
