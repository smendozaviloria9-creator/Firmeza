# Firmeza — Sistema de gestion de materiales de construccion

Panel administrativo ASP.NET Core MVC para una empresa de venta de
materiales de construccion, con autenticacion por roles, importacion
y exportacion masiva de datos, generacion de recibos PDF, y arquitectura
limpia (Clean Architecture) organizada en 4 capas.

## Stack tecnico

| Capa            | Tecnologia                                          |
|------------------|------------------------------------------------------|
| Backend/Panel    | ASP.NET Core 10 MVC (Razor Views)                    |
| Base de datos    | PostgreSQL 16 + Npgsql.EntityFrameworkCore           |
| ORM              | Entity Framework Core 10 (Code First + Migraciones)  |
| Autenticacion    | ASP.NET Core Identity (roles: Administrador, Cliente)|
| Importacion/Excel| EPPlus                                                |
| Exportacion/PDF  | QuestPDF                                              |
| UI               | Bootstrap 5 + Bootstrap Icons                        |
| Pruebas          | xUnit                                                 |
| Empaquetado      | Docker + docker-compose                              |

## Arquitectura (Clean Architecture)

El proyecto esta organizado en 4 capas, cada una en su propio proyecto
.NET, con las dependencias fluyendo en una sola direccion:

```
Firmeza.Web --> Firmeza.Application
Firmeza.Web --> Firmeza.Infrastructure
Firmeza.Infrastructure --> Firmeza.Application
Firmeza.Application --> Firmeza.Domain
Firmeza.Infrastructure --> Firmeza.Domain
```

- **Firmeza.Domain**: entidades puras del negocio (Producto, Cliente,
  Venta, DetalleVenta, Roles), sin dependencias externas.
- **Firmeza.Application**: interfaces de los servicios de la aplicacion
  (IExcelImportService, IExportService, IReciboService) y DTOs
  (ImportResultViewModel).
- **Firmeza.Infrastructure**: implementacion concreta de EF Core
  (ApplicationDbContext, DbInitializer, Migrations), Identity
  (ApplicationUser), y los 3 servicios (ExcelImportService,
  ExportService, ReciboService).
- **Firmeza.Web**: controladores, vistas Razor, ViewModels y el punto
  de composicion (Program.cs) donde se registran las implementaciones
  concretas contra sus interfaces.

Ver los diagramas completos en `docs/diagrama-de-clases.md` y
`docs/modelo-entidad-relacion.md`.

## Roles del sistema

- **Administrador**: puede iniciar sesion en el panel Razor y gestionar
  productos, clientes y ventas.
- **Cliente**: existe en la base de Identity, pero no puede iniciar
  sesion en este panel. Esta pensado para una futura app de clientes
  separada.

## Como correrlo en local (sin Docker)

### 1. Requisitos
- .NET SDK 10.0+
- PostgreSQL 16 corriendo localmente (o en un contenedor)
- Herramientas de EF Core: `dotnet tool install --global dotnet-ef`

### 2. Configurar la cadena de conexion
Edita `src/Firmeza.Web/appsettings.Development.json`, o usa
`dotnet user-secrets` para no subir credenciales al repositorio:

```bash
cd src/Firmeza.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=firmeza_db;Username=firmeza_user;Password=TU_CLAVE"
```

### 3. Restaurar paquetes y aplicar migraciones

```bash
dotnet restore
cd src/Firmeza.Web
dotnet ef database update
```

### 4. Ejecutar la aplicacion

```bash
dotnet run
```

Al arrancar, `DbInitializer` aplica automaticamente las migraciones
pendientes y siembra los roles (`Administrador`, `Cliente`) y un usuario
administrador inicial, configurable en `appsettings.json` seccion
`AdminSeed` (por defecto `admin@firmeza.com` / `Admin123$`).

Abre `http://localhost:5000` (o el puerto que indique la consola).

## Como correrlo con Docker

Crea un archivo `.env` en la raiz del proyecto con la contrasena de
PostgreSQL:

```bash
echo "DB_PASSWORD=TU_CLAVE" > .env
```

Levanta los contenedores:

```bash
docker compose up -d --build
```

Esto levanta:
- `db`: PostgreSQL 16 con la base `firmeza_db`, con healthcheck.
- `web`: la app ASP.NET Core, escuchando en `http://localhost:8080`.

El contenedor `web` espera a que `db` este saludable antes de arrancar
(gracias al healthcheck y `depends_on: condition: service_healthy`), y
aplica las migraciones y el seed automaticamente al iniciar.

## Funcionalidades de la Semana 2

- **Importacion masiva desde Excel**: carga un archivo `.xlsx` con
  columnas de cliente, producto y venta mezcladas; el sistema normaliza,
  valida e inserta/actualiza los registros, mostrando un log de errores
  por fila (`ImportacionController`, `ExcelImportService`).
- **Exportacion a Excel y PDF**: Productos, Clientes y Ventas se pueden
  exportar en ambos formatos desde sus respectivas vistas
  (`ExportacionController`, `ExportService`).
- **Recibos PDF**: cada venta genera automaticamente un recibo en PDF
  (`ReciboService`), guardado en `wwwroot/recibos` y descargable desde
  el detalle de la venta.
- **Descuento de stock**: al registrar una venta, el stock del producto
  se descuenta segun la cantidad vendida.
- **Diseno visual uniforme**: sidebar, encabezado y pie de pagina
  consistentes en todas las vistas del panel.

## Pruebas unitarias

```bash
cd src/Firmeza.Tests
dotnet test
```

## Notas de seguridad

El archivo `.env` (con la contrasena de la base de datos) y las carpetas
`bin/`, `obj/` estan excluidas del control de versiones via
`.gitignore`. Nunca subas credenciales reales al repositorio.
