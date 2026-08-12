# Guía de despliegue — Pedido360

## Requisitos previos

Antes de ejecutar el proyecto se debe contar con:

* Visual Studio 2022 o superior.
* .NET SDK 9.0.
* SQL Server Express LocalDB.
* Git.
* Entity Framework Core Tools.

## 1. Clonar el repositorio

Abrir una terminal o Git Bash y ejecutar:

```bash
git clone https://github.com/eelizondogene-rgb/Pedido360.git
```

Ingresar a la carpeta del proyecto:

```bash
cd Pedido360/Pedido360
```

## 2. Restaurar dependencias

Ejecutar:

```bash
dotnet restore
```

Esto descargará las dependencias necesarias definidas por el proyecto.

## 3. Crear y actualizar la base de datos

El proyecto utiliza Entity Framework Core mediante el enfoque Code First.

Desde la terminal ejecutar:

```bash
dotnet ef database update
```

Este comando crea la base de datos en SQL Server LocalDB y aplica las migraciones existentes.

También puede realizarse desde Visual Studio mediante:

**Tools → NuGet Package Manager → Package Manager Console**

Y ejecutar:

```powershell
Update-Database
```

Al iniciar el proyecto por primera vez, `DbInitializer` crea automáticamente los roles, usuarios y datos iniciales requeridos para realizar pruebas.

## 4. Ejecutar el proyecto

Desde la terminal:

```bash
dotnet run
```

También puede abrirse el archivo:

```text
Pedido360.sln
```

en Visual Studio y ejecutar la aplicación utilizando **F5** o el botón **Run**.

## 5. Usuarios y roles de prueba

| Rol           | Usuario                     | Contraseña        |
| ------------- | --------------------------- | ----------------- |
| Administrador | `admin@pedido360.com`       | `Admin123!`       |
| Ventas        | `ventas@pedido360.com`      | `Ventas123!`      |
| Operaciones   | `operaciones@pedido360.com` | `Operaciones123!` |

### Administrador

Posee acceso completo al sistema y puede administrar productos, clientes y pedidos.

### Ventas

Puede consultar productos y clientes, registrar clientes y crear, confirmar o cancelar pedidos.

### Operaciones

Puede consultar información y administrar productos e inventario, pero no puede crear pedidos ni eliminar productos o clientes.

## Resumen rápido

Para levantar el proyecto desde cero:

```bash
git clone https://github.com/eelizondogene-rgb/Pedido360.git
cd Pedido360/Pedido360
dotnet restore
dotnet ef database update
dotnet run
```

Posteriormente se puede iniciar sesión utilizando cualquiera de los usuarios de prueba indicados anteriormente.
