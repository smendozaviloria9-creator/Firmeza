# Diagrama de clases — Firmeza (Clean Architecture)

## Capa Domain (Firmeza.Domain)

```mermaid
classDiagram
    class Cliente {
        +int Id
        +string Nombres
        +string Apellidos
        +string Documento
        +string Correo
        +string Telefono
        +string Direccion
        +int Edad
        +datetime FechaRegistro
        +ICollection~Venta~ Ventas
    }

    class Producto {
        +int Id
        +string Nombre
        +string Descripcion
        +string Categoria
        +string UnidadMedida
        +decimal PrecioUnitario
        +int Stock
        +bool Activo
        +ICollection~DetalleVenta~ DetallesVenta
    }

    class Venta {
        +int Id
        +int ClienteId
        +datetime Fecha
        +decimal Subtotal
        +decimal Iva
        +decimal Total
        +string ReciboArchivo
        +ICollection~DetalleVenta~ Detalles
    }

    class DetalleVenta {
        +int Id
        +int VentaId
        +int ProductoId
        +int Cantidad
        +decimal PrecioUnitario
        +decimal Subtotal
    }

    class Roles {
        +string Administrador$
        +string Cliente$
    }

    Cliente "1" --> "many" Venta
    Venta "1" --> "many" DetalleVenta
    Producto "1" --> "many" DetalleVenta
```

## Capa Application (Firmeza.Application)

```mermaid
classDiagram
    class IExcelImportService {
        +ImportarAsync(Stream) Task~ImportResultViewModel~
    }

    class IExportService {
        +ExportarProductosExcelAsync() Task~byte[]~
        +ExportarProductosPdfAsync() Task~byte[]~
        +ExportarClientesExcelAsync() Task~byte[]~
        +ExportarClientesPdfAsync() Task~byte[]~
        +ExportarVentasExcelAsync() Task~byte[]~
        +ExportarVentasPdfAsync() Task~byte[]~
    }

    class IReciboService {
        +GenerarRecibo(Venta, string) string
    }

    class ImportResultViewModel {
        +int ClientesInsertados
        +int ClientesActualizados
        +int ProductosInsertados
        +int ProductosActualizados
        +int VentasInsertadas
        +List~string~ Errores
        +bool TuvoErrores
    }
```

## Capa Infrastructure (Firmeza.Infrastructure)

```mermaid
classDiagram
    class ApplicationDbContext {
        +DbSet~Producto~ Productos
        +DbSet~Cliente~ Clientes
        +DbSet~Venta~ Ventas
        +DbSet~DetalleVenta~ DetallesVenta
    }

    class ApplicationUser {
        +string Id
        +string Email
        +string NombreCompleto
        +int? ClienteId
    }

    class DbInitializer {
        +SeedAsync(IServiceProvider, IConfiguration) Task
    }

    class ExcelImportService {
        +ImportarAsync(Stream) Task~ImportResultViewModel~
    }

    class ExportService {
        +ExportarProductosExcelAsync() Task~byte[]~
        +ExportarVentasExcelAsync() Task~byte[]~
    }

    class ReciboService {
        +decimal PorcentajeIva$
        +GenerarRecibo(Venta, string) string
    }

    ApplicationUser "1" --> "0..1" Cliente
    ApplicationDbContext ..> Cliente
    ApplicationDbContext ..> Producto
    ApplicationDbContext ..> Venta
    ApplicationDbContext ..> DetalleVenta
    ExcelImportService ..|> IExcelImportService
    ExportService ..|> IExportService
    ReciboService ..|> IReciboService
    ExcelImportService --> ApplicationDbContext
    ExportService --> ApplicationDbContext
```

## Direccion de dependencias

```mermaid
flowchart LR
    Web[Firmeza.Web] --> Application[Firmeza.Application]
    Web --> Infrastructure[Firmeza.Infrastructure]
    Infrastructure --> Application
    Application --> Domain[Firmeza.Domain]
    Infrastructure --> Domain
```

Firmeza.Domain no depende de ningun otro proyecto. Firmeza.Application
solo depende de Domain. Firmeza.Infrastructure implementa las interfaces
de Application y depende de EF Core, Npgsql, Identity, EPPlus y QuestPDF.
Firmeza.Web es el punto de composicion: registra las implementaciones
concretas y expone los controladores y vistas.
