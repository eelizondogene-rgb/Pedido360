# Guia de Git para el equipo — Pedido360

Esta guia explica, paso a paso, como subir el proyecto a un repositorio compartido y como debe trabajar cada integrante del equipo sin pisarse el codigo entre si. Esta pensada para gente que nunca ha usado Git en equipo.

---

## 0. Antes de empezar

Cada integrante necesita:

1. **Git instalado.** Verificar con:
   ```bash
   git --version
   ```
   Si no aparece nada, descargar de https://git-scm.com/downloads

2. **Una cuenta de GitHub** (o GitLab, pero esta guia usa GitHub como ejemplo). Recomendado: que todos usen su correo de Fidelitas o uno personal, no importa, pero que sea consistente.

3. **Git configurado con su nombre real** (para que las commits identifiquen a cada persona, esto cuenta para la nota de "control de versiones" del rubro):
   ```bash
   git config --global user.name "Nombre Apellido"
   git config --global user.email "correo@ejemplo.com"
   ```

---

## 1. Crear el repositorio (lo hace UNA sola persona, el lider tecnico)

### 1.1 Crear el repo en GitHub
1. Entrar a https://github.com → boton verde **New repository**.
2. Nombre sugerido: `Pedido360`.
3. Visibilidad: **Private** (es un proyecto de curso, no necesita ser publico) — luego se agregan los compañeros como colaboradores.
4. **No** marcar "Add a README" ni ".gitignore" si ya tienen el proyecto armado localmente (evita conflictos al primer push). Dejar el repo vacio.
5. Clic en **Create repository**. GitHub mostrara una URL como:
   ```
   https://github.com/tu-usuario/Pedido360.git
   ```

### 1.2 Agregar a los compañeros como colaboradores
En el repo → **Settings** → **Collaborators** → **Add people** → buscar por su usuario o correo de GitHub de cada integrante. Cada uno debe aceptar la invitacion (les llega un correo o notificacion).

### 1.3 Subir el proyecto ya unificado por primera vez
Parado dentro de la carpeta `Pedido360/` (la que contiene `Pedido360.csproj`):

```bash
git init
git add .
git commit -m "Version inicial: base del proyecto + modulo de Clientes integrado"
git branch -M main
git remote add origin https://github.com/tu-usuario/Pedido360.git
git push -u origin main
```

> Si pide usuario/contraseña y la rechaza: GitHub ya no acepta contraseña normal por linea de comandos. Hay que generar un **Personal Access Token** (Settings → Developer settings → Personal access tokens → Generate new token, marcar el permiso `repo`) y usarlo como contraseña esa unica vez, o configurar GitHub Desktop / SSH.

---

## 2. Cada compañero clona el repo (todos, una sola vez)

```bash
git clone https://github.com/tu-usuario/Pedido360.git
cd Pedido360
```

Esto descarga una copia completa del proyecto con todo el historial.

### 2.1 Verificar que el proyecto compila tal cual se clono
```bash
dotnet restore
dotnet build
```

### 2.2 Aplicar las migraciones a su base de datos local
```bash
dotnet ef database update
```
(Si no tienen la herramienta `dotnet ef` instalada: `dotnet tool install --global dotnet-ef`)

### 2.3 Correr el proyecto
```bash
dotnet run
```

Si todo esto funciona para los 5, ya tienen el ambiente listo para trabajar en paralelo.

---

## 3. Flujo de trabajo diario: ramas (branches)

**Regla de oro: nadie trabaja directamente sobre `main`.** `main` siempre debe tener una version que compila y funciona — es lo que se entrega/demuestra.

Cada persona trabaja en su propia rama, nombrada segun su parte:

| Integrante | Rama sugerida |
|---|---|
| Backend / EF / Migrations | `feature/backend-ef` |
| Productos | `feature/productos` |
| Clientes | `feature/clientes` |
| Pedidos + AJAX | `feature/pedidos-ajax` |
| Seguridad / Identity / Roles | `feature/seguridad` |
| QA / Docs / Seeders | `feature/qa-docs` |

### 3.1 Crear y moverse a tu rama
```bash
git checkout -b feature/clientes
```

### 3.2 Trabajar normalmente
Edita archivos como siempre. Cuando quieras guardar un avance:

```bash
git add .
git commit -m "Agrega validacion de cedula duplicada en ClientesController"
```

**Buenas practicas de mensajes de commit:**
- Cortos, en presente, describiendo QUE se hizo: `"Agrega CRUD de Categorias"`, no `"cambios"` o `"asdf"`.
- Hacer commits pequeños y frecuentes (cada funcionalidad terminada), no un commit gigante al final — la rubrica pide ver "contribucion real de cada miembro".

### 3.3 Subir tu rama al repositorio remoto
```bash
git push -u origin feature/clientes
```
(la primera vez; despues solo `git push`)

---

## 4. Integrar tu trabajo a `main`: Pull Requests

Cuando tu parte ya funciona y quieres unirla al proyecto principal:

1. Entra al repo en GitHub.
2. Aparecera un boton **Compare & pull request** sobre tu rama recien subida. Clic ahi.
3. Escribe un titulo claro (ej: "Modulo de Clientes: CRUD completo + busqueda") y una breve descripcion de que incluye.
4. Asigna a otro compañero como **revisor** (Reviewers, a la derecha).
5. El revisor entra, revisa los cambios en la pestaña **Files changed**, y:
   - Si esta bien: aprueba con **Approve**.
   - Si hay algo que corregir: deja comentarios con **Request changes**.
6. Una vez aprobado, el autor (o quien tenga permisos) le da **Merge pull request**.

Esto es lo que la rubrica llama "PRs/revisiones" — generar varios Pull Requests durante el curso (no solo al final) demuestra trabajo en equipo real.

---

## 5. Mantener tu rama actualizada (evitar conflictos grandes)

Como varios trabajan a la vez, conviene traer los cambios de `main` a tu rama seguido, no solo al final:

```bash
git checkout main
git pull
git checkout feature/clientes
git merge main
```

Si Git marca un **conflicto** (mismas lineas tocadas por dos personas), abrira los archivos con marcas como:

```
<<<<<<< HEAD
tu version
=======
version de main
>>>>>>> main
```

Hay que editar a mano, decidir que queda, borrar esas marcas, y luego:
```bash
git add .
git commit -m "Resuelve conflicto entre feature/clientes y main"
```

---

## 6. Checklist rapido del dia a dia

```bash
# Al empezar a trabajar
git checkout main
git pull
git checkout feature/tu-rama
git merge main          # traer lo nuevo de main a tu rama

# Mientras trabajas
git add .
git commit -m "mensaje claro"

# Al terminar una sesion / funcionalidad
git push
```

---

## 7. Sobre el caso especifico de Clientes (lo que ya integramos)

El modulo de Clientes ya fue fusionado dentro del proyecto base `Pedido360`:
- `Models/Cliente.cs`, `Controllers/ClientesController.cs`, `ViewModels/*`, `Views/Clientes/*`
- Una migracion nueva `AddClienteCamposExtra` que agrega los campos `Activo` y `FechaDeRegistro`, y un indice unico sobre `Cedula`.
- 8 clientes de prueba se cargan automaticamente la primera vez que se corre el proyecto (via `DbInitializer`), no falta correr ningun script `.sql` aparte.

**Importante para quien revise:** no se pudo compilar el proyecto en el entorno donde se hizo esta integracion (no habia SDK de .NET disponible). Se recomienda que la primera persona que clone el repo corra `dotnet build` y `dotnet ef database update` y reporte si algo falla, antes de que el resto siga construyendo sobre esta base.

---

## 8. Comandos de emergencia (por si algo sale mal)

| Situacion | Comando |
|---|---|
| Ver en que rama estoy | `git status` |
| Ver historial de commits | `git log --oneline --graph --all` |
| Descartar cambios locales no guardados | `git checkout -- nombre-archivo` |
| Deshacer el ultimo commit (sin perder cambios) | `git reset --soft HEAD~1` |
| Ver diferencias antes de hacer commit | `git diff` |
| Traer la version mas reciente sin perder mi trabajo | `git stash` → `git pull` → `git stash pop` |

Si algo se ve realmente mal y nadie sabe que paso, **no forzar nada** (`git push --force`) sin preguntar al equipo primero — eso puede borrar el trabajo de otros.
