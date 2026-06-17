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
        }
    }
}
