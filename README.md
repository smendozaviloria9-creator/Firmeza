# Firmeza — Módulo Administrativo Base

Sistema de gestión para un negocio de venta y distribución de materiales de
construcción. Semana 1: infraestructura, panel Razor, gestión de productos
y clientes, con autenticación por roles (Identity).

## Stack técnico

| Capa            | Tecnología                                   |
|-----------------|-----------------------------------------------|
| Backend/Panel   | ASP.NET Core 10 MVC (Razor Views)              |
| Base de datos   | PostgreSQL 16 + Npgsql.EntityFrameworkCore    |
| ORM             | Entity Framework Core 10 (Code First + Migraciones) |
| Autenticación   | ASP.NET Core Identity (roles: Administrador, Cliente) |
| UI              | Bootstrap 5 + Bootstrap Icons                 |
| Pruebas         | xUnit                                         |
| Empaquetado     | Docker + docker-compose                       |
| Reservado (futuro) | EPPlus (Excel), QuestPDF (PDF)             |

## Estructura del proyecto

```
Firmeza/
├─ Firmeza.sln
├─ Dockerfile
├─ docker-compose.yml
├─ docs/
│  ├─ modelo-entidad-relacion.md   (diagrama ER en Mermaid)
│  └─ diagrama-de-clases.md        (diagrama de clases en Mermaid)
└─ src/
   ├─ Firmeza.Web/            → proyecto ASP.NET Core Web (el panel)
   │  ├─ Controllers/         → Home, Account, Productos, Clientes
   │  ├─ Data/                → ApplicationDbContext, DbInitializer (seed)
   │  ├─ Models/               → Producto, Cliente, Venta, DetalleVenta, ApplicationUser, Roles
   │  ├─ ViewModels/           → ProductoViewModel, ClienteViewModel
   │  ├─ Utils/EdadValidator.cs → validación con try/catch (Task 7)
   │  └─ Views/                → Razor views (Bootstrap 5, sidebar)
   └─ Firmeza.Tests/          → pruebas unitarias con xUnit
```

## Roles del sistema

- **Administrador**: puede iniciar sesión en el panel Razor y gestionar
  productos y clientes.
- **Cliente**: existe en la base de Identity, pero **no puede** iniciar
  sesión en este panel. Si lo intenta, `AccountController.Login` detecta
  que no tiene el rol `Administrador`, cierra la sesión inmediatamente y
  muestra un mensaje indicando que debe usar la futura app de clientes
  (Blazor/React/Angular/Vue, fuera del alcance de esta semana).

## Cómo correrlo en local (sin Docker)

### 1. Requisitos
- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- PostgreSQL 16 corriendo localmente (o en un contenedor)
- Herramientas de EF Core: `dotnet tool install --global dotnet-ef`

### 2. Configurar la cadena de conexión
Edita `src/Firmeza.Web/appsettings.json` (o mejor, usa `dotnet user-secrets`
para no subir credenciales al repositorio):

```bash
cd src/Firmeza.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=firmeza_db;Username=firmeza_user;Password=TU_CLAVE"
```

### 3. Restaurar paquetes
```bash
dotnet restore
```

### 4. Crear la migración inicial (requisito: solo con EF Core, sin SQL manual)
```bash
cd src/Firmeza.Web
dotnet ef migrations add InicialAdmin -o Data/Migrations
```
> No se incluyen migraciones pre-generadas en este entrega porque dependen
> de la versión exacta del SDK/herramientas de quien las genera. Al correr
> `dotnet ef migrations add`, EF Core crea el snapshot del modelo (tablas
> `Productos`, `Clientes`, `Ventas`, `DetallesVenta` + tablas de Identity).

### 5. Ejecutar la aplicación
```bash
dotnet run
```
Al arrancar, `DbInitializer` aplica automáticamente las migraciones
pendientes (`Database.MigrateAsync()`) y siembra:
- Los roles `Administrador` y `Cliente`.
- Un usuario administrador inicial (configurable en `appsettings.json`,
  sección `AdminSeed`; por defecto `admin@firmeza.com` / `Admin123$`).

Abre `https://localhost:5001` (o el puerto que indique la consola) e
inicia sesión con esas credenciales.

## Cómo correrlo con Docker

```bash
docker compose up --build
```

Esto levanta:
- `db`: PostgreSQL 16 con la base `firmeza_db`.
- `web`: la app ASP.NET Core, escuchando en `http://localhost:8080`.

Al iniciar el contenedor `web`, se aplican las migraciones y se siembra el
usuario administrador igual que en local. Si aún no generaste la carpeta
de migraciones, hazlo primero en tu máquina (paso 4 de arriba) y luego
haz commit de esos archivos antes de construir la imagen, ya que el
Dockerfile solo copia el código fuente, no ejecuta `migrations add`.

> El `docker-compose.yml` y el `Dockerfile` son un punto de partida
> (Task 12, marcada como opcional para esta semana); en semanas
> posteriores se puede robustecer con healthchecks, multi-entorno, etc.

## Pruebas unitarias

```bash
cd src/Firmeza.Tests
dotnet test
```

Se incluye `EdadValidatorTests`, que cubre la conversión segura de la edad
de un cliente (texto → entero) usando xUnit, incluyendo casos de error
(texto no numérico, fuera de rango, overflow).

## Notas sobre las tareas de la historia de usuario

| Task | Estado | Dónde está |
|------|--------|------------|
| 1. Proyecto Razor + dependencias | ✅ | `Firmeza.Web.csproj` |
| 2. PostgreSQL + migraciones | ✅ | `Data/ApplicationDbContext.cs`, `Program.cs` |
| 3. Entidades base | ✅ | `Models/*.cs` |
| 4. Identity + roles | ✅ | `Data/DbInitializer.cs`, `Controllers/AccountController.cs` |
| 5. Dashboard | ✅ | `Controllers/HomeController.cs`, `Views/Home/Index.cshtml` |
| 6. CRUD productos | ✅ | `Controllers/ProductosController.cs`, `Views/Productos/*` |
| 7. Try-catch edad | ✅ | `Utils/EdadValidator.cs`, usado en `ClientesController` |
| 8. CRUD clientes | ✅ | `Controllers/ClientesController.cs`, `Views/Clientes/*` |
| 9. Diseño visual | ✅ | `Views/Shared/_Layout.cshtml`, `wwwroot/css/site.css` |
| 10. Documentación | ✅ | Este `README.md` + `docs/` |
| 11. Pruebas xUnit | ✅ | `Firmeza.Tests/EdadValidatorTests.cs` |
| 12. Docker | ✅ (borrador) | `Dockerfile`, `docker-compose.yml` |
