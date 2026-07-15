# Pedidos360 — Inventario y Pedidos B2B

Proyecto final del curso SC-601 Programación Avanzada (Universidad Fidélitas).
Aplicación ASP.NET Core MVC para que una pyme gestione catálogo de productos,
clientes e inventario, y arme pedidos con cálculo automático de totales.

## Stack

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core (Code-First + Migrations) sobre SQL Server LocalDB
- ASP.NET Core Identity (autenticación + roles)
- Bootstrap 5 + jQuery + jQuery Validation Unobtrusive
- AJAX nativo (`fetch`) para autosuggest de productos y cálculo de totales en vivo

## Requisitos previos

- .NET SDK 9.0
- SQL Server LocalDB (viene con Visual Studio; si no, instalar "SQL Server
  Express LocalDB" por separado)

## Puesta en marcha (instalación express)

```bash
git clone <url-del-repo>
cd Pedido360/Pedido360
dotnet restore
dotnet ef database update      # crea la base de datos y aplica las migraciones
dotnet run
```

Al iniciar por primera vez, `DbInitializer` siembra automáticamente:

- Los 3 roles del sistema (`Admin`, `Ventas`, `Operaciones`) y un usuario por rol.
- 8 clientes de ejemplo.
- 4 categorías y 12 productos con precio, impuesto y stock realistas (uno de
  ellos a propósito con stock bajo, para ver la alerta del panel).

Si preferís usar Visual Studio: abrir `Pedido360.sln`, `Update-Database` desde
la consola de Package Manager, y `F5`.

## Usuarios de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Admin | `admin@pedido360.com` | `Admin123!` |
| Ventas | `ventas@pedido360.com` | `Ventas123!` |
| Operaciones | `operaciones@pedido360.com` | `Operaciones123!` |

## Permisos por rol

| Acción | Admin | Ventas | Operaciones |
|---|:---:|:---:|:---:|
| Ver catálogo / clientes / pedidos | ✅ | ✅ | ✅ |
| Crear / editar productos | ✅ | ❌ | ✅ |
| Eliminar productos | ✅ | ❌ | ❌ |
| Crear / editar clientes | ✅ | ✅ | ❌ |
| Eliminar clientes | ✅ | ❌ | ❌ |
| Crear / confirmar / cancelar pedidos | ✅ | ✅ | ❌ |

## Flujo sugerido para la demo

1. Entrar como **Admin**, revisar el panel y el catálogo (filtro por
   categoría + paginación).
2. Crear un producto nuevo (la imagen es obligatoria al crear).
3. Entrar como **Ventas**, crear un cliente y buscarlo.
4. Armar un pedido: buscar productos por nombre (autosuggest AJAX),
   ajustar cantidad/descuento y ver el total recalcularse en vivo
   (`POST /api/pedidos/calcular`).
5. Confirmar el pedido y comprobar que el stock del producto bajó.
6. Ver el detalle del pedido con el desglose de líneas.
7. Probar entrar como **Operaciones**: puede editar inventario pero no ve el
   botón de crear pedido ni de eliminar productos/clientes.

## Endpoints de API

- `GET /api/productos/buscar?q=texto` → hasta 10 productos activos con stock,
  para el autosuggest del formulario de pedidos.
- `POST /api/pedidos/calcular` → recibe `{ lineas: [{ productoId, cantidad,
  descuento }] }` y devuelve subtotal, descuentos, impuestos y total
  recalculados con los precios vigentes en base de datos. Ambos requieren
  sesión iniciada (`[Authorize]`).

## Arquitectura y decisiones de diseño

- **Capas**: Controllers → `ApplicationDbContext` (EF Core) directamente,
  con ViewModels para desacoplar las vistas del modelo de datos. La lógica de
  cálculo de totales vive en `Services/PedidoMath.cs`, compartida entre el
  endpoint de API y el `PedidosController`, para que el número que ve el
  usuario en vivo sea siempre el mismo que se termina persistiendo.
- **Auditoría**: los pedidos guardan `Subtotal`, `Impuestos`, `Total` y cada
  línea su `PrecioUnit`/`ImpuestoPorc`/`Descuento` al momento de la compra,
  aunque el precio del producto cambie después.
- **Stock**: se descuenta únicamente al **confirmar** un pedido (no al
  crearlo), y se repone si un pedido confirmado se cancela.
- **Normalización**: el modelo (`Producto`, `Categoria`, `Cliente`, `Pedido`,
  `PedidoDetalle`) está en 3FN — cada tabla no clave depende solo de la llave
  primaria, sin atributos derivados salvo los totales de auditoría en
  `Pedido`/`PedidoDetalle`, que se recalculan pero se persisten a propósito.

## Estructura del proyecto

```
Pedido360/
├── Controllers/          MVC (Home, Clientes, Productos, Pedidos)
├── Controllers/Api/       API JSON (ProductosApi, PedidosApi)
├── Services/               PedidoMath (formula de calculo compartida)
├── Models/                 Entidades EF Core
├── ViewModels/              DTOs para las vistas y la API
├── Views/                   Razor views
├── Data/                     ApplicationDbContext + DbInitializer (seed)
└── wwwroot/css/theme.css     Identidad visual del proyecto
```

## Pendiente / posibles mejoras (bono)

- Exportar pedido a PDF/Excel.
- Bitácora de auditoría de cambios.
- Reporte de ventas por fecha/cliente.
- Miniaturas y validación de tamaño de imagen (hoy `ImagenUrl` es una URL
  externa, no un archivo subido).
