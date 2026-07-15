using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pedido360.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pedido360.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roleNames = { "Admin", "Ventas", "Operaciones" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create a default admin user
            var adminUser = await userManager.FindByEmailAsync("admin@pedido360.com");
            if (adminUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin@pedido360.com",
                    Email = "admin@pedido360.com",
                    EmailConfirmed = true
                };

                var createPowerUser = await userManager.CreateAsync(user, "Admin123!");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Usuarios de prueba para los otros dos roles (checklist de entrega)
            var usuariosDePrueba = new (string Email, string Password, string Rol)[]
            {
                ("ventas@pedido360.com", "Ventas123!", "Ventas"),
                ("operaciones@pedido360.com", "Operaciones123!", "Operaciones")
            };

            foreach (var (email, password, rol) in usuariosDePrueba)
            {
                var existente = await userManager.FindByEmailAsync(email);
                if (existente is not null)
                    continue;

                var nuevoUsuario = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(nuevoUsuario, password);
                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(nuevoUsuario, rol);
                }
            }

            // Seed de clientes de prueba (modulo Clientes)
            if (!await context.Clientes.AnyAsync())
            {
                context.Clientes.AddRange(
                    new Cliente { Nombre = "Maria Gonzalez Ramirez", Cedula = "1-0501-0123", Correo = "maria.gonzalez@correo.com", Telefono = "8800-1111", Direccion = "San Jose, Desamparados", FechaDeRegistro = DateTime.Now, Activo = true },
                    new Cliente { Nombre = "Carlos Mora Jimenez", Cedula = "1-0602-0234", Correo = "carlos.mora@empresa.cr", Telefono = "8800-2222", Direccion = "Heredia, San Francisco", FechaDeRegistro = DateTime.Now, Activo = true },
                    new Cliente { Nombre = "Ana Vargas Solano", Cedula = "3-0401-0345", Correo = "ana.vargas@gmail.com", Telefono = "8800-3333", Direccion = "Cartago, La Union", FechaDeRegistro = DateTime.Now, Activo = true },
                    new Cliente { Nombre = "Luis Rojas Castro", Cedula = "2-0301-0456", Correo = "luis.rojas@hotmail.com", Telefono = "8800-4444", Direccion = "Alajuela, San Carlos", FechaDeRegistro = DateTime.Now, Activo = true },
                    new Cliente { Nombre = "Empresa XYZ S.A.", Cedula = "3-101-123456", Correo = "facturacion@xyz.cr", Telefono = "2200-5555", Direccion = "Zona Franca, San Jose", FechaDeRegistro = DateTime.Now, Activo = true },
                    new Cliente { Nombre = "Distribuidora ABC Ltda.", Cedula = "3-102-234567", Correo = "compras@abc.cr", Telefono = "2200-6666", Direccion = "La Uruca, San Jose", FechaDeRegistro = DateTime.Now, Activo = true },
                    new Cliente { Nombre = "Sofia Mendez Arias", Cedula = "4-0201-0567", Correo = "sofia.mendez@correo.com", Telefono = "8800-7777", Direccion = "Liberia, Guanacaste", FechaDeRegistro = DateTime.Now, Activo = true },
                    new Cliente { Nombre = "Roberto Salas Vega", Cedula = "5-0101-0678", Correo = "roberto.salas@outlook.com", Telefono = "8800-8888", Direccion = "Puntarenas, Esparza", FechaDeRegistro = DateTime.Now, Activo = true }
                );
                await context.SaveChangesAsync();
            }

            // Seed de categorias y productos de prueba (modulo Productos / Pedidos)
            if (!await context.Categorias.AnyAsync())
            {
                var categorias = new[]
                {
                    new Categoria { Nombre = "Abarrotes" },
                    new Categoria { Nombre = "Limpieza" },
                    new Categoria { Nombre = "Bebidas" },
                    new Categoria { Nombre = "Papeleria" }
                };
                context.Categorias.AddRange(categorias);
                await context.SaveChangesAsync();

                var abarrotes = categorias[0];
                var limpieza = categorias[1];
                var bebidas = categorias[2];
                var papeleria = categorias[3];

                context.Productos.AddRange(
                    new Producto { Nombre = "Arroz 1kg", CategoriaId = abarrotes.Id, Precio = 950, ImpuestoPorc = 1, Stock = 120, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=400" },
                    new Producto { Nombre = "Frijol negro 900g", CategoriaId = abarrotes.Id, Precio = 1250, ImpuestoPorc = 1, Stock = 80, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1611575619752-0d7dfbe2d3ce?w=400" },
                    new Producto { Nombre = "Aceite vegetal 1L", CategoriaId = abarrotes.Id, Precio = 1800, ImpuestoPorc = 13, Stock = 60, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=400" },
                    new Producto { Nombre = "Azucar blanca 1kg", CategoriaId = abarrotes.Id, Precio = 850, ImpuestoPorc = 1, Stock = 95, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1610725664285-7c57e6eeac3f?w=400" },
                    new Producto { Nombre = "Cafe molido 500g", CategoriaId = abarrotes.Id, Precio = 2600, ImpuestoPorc = 13, Stock = 50, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=400" },
                    new Producto { Nombre = "Detergente en polvo 1kg", CategoriaId = limpieza.Id, Precio = 2400, ImpuestoPorc = 13, Stock = 45, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1585421514738-01798e348b17?w=400" },
                    new Producto { Nombre = "Cloro 1L", CategoriaId = limpieza.Id, Precio = 900, ImpuestoPorc = 13, Stock = 70, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1563453392212-326f5e854473?w=400" },
                    new Producto { Nombre = "Jabon de mano liquido 750ml", CategoriaId = limpieza.Id, Precio = 1600, ImpuestoPorc = 13, Stock = 55, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1584622781564-1d987f7333c1?w=400" },
                    new Producto { Nombre = "Gaseosa 2L", CategoriaId = bebidas.Id, Precio = 1500, ImpuestoPorc = 13, Stock = 100, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1554866585-cd94860890b7?w=400" },
                    new Producto { Nombre = "Agua embotellada 600ml (paquete x6)", CategoriaId = bebidas.Id, Precio = 2100, ImpuestoPorc = 13, Stock = 90, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1560023907-5f339617ea30?w=400" },
                    new Producto { Nombre = "Jugo de naranja 1L", CategoriaId = bebidas.Id, Precio = 1350, ImpuestoPorc = 13, Stock = 65, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=400" },
                    new Producto { Nombre = "Resma de papel carta", CategoriaId = papeleria.Id, Precio = 3200, ImpuestoPorc = 13, Stock = 4, Activo = true, ImagenUrl = "https://images.unsplash.com/photo-1568205612837-017257d2310a?w=400" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
