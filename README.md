# Firmeza Concesionario — Sistema Integral de Gestión de Vehículos, Rentas y Repuestos

Sistema empresarial para la gestión de un **concesionario automotriz** desarrollado con arquitectura limpia (**Clean Architecture**) en 4 capas, que cubre dos líneas clave de negocio:
1. **Vehículos:** Venta de vehículos nuevos/usados y alquiler / renta de vehículos por día.
2. **Repuestos y Autopartes:** Catálogo, control de stock y ventas de repuestos automotrices (reemplazando los materiales tradicionales por componentes automotrices como frenos, motor, suspensión, baterías, etc.).

El ecosistema está compuesto por:
* **API RESTful** (`Firmeza.Api`): ASP.NET Core 10 con autenticación JWT, documentación Swagger interactiva, AutoMapper, exportación masiva y servicio de notificaciones SMTP.
* **Panel Web Administrativo** (`Firmeza.Web`): ASP.NET Core 10 MVC con autenticación por roles, importación/exportación masiva Excel y generación de recibos PDF.
* **Base de datos relacional:** PostgreSQL 16 con Entity Framework Core 10 (compatible en local con Docker o en la nube con **Supabase**).
* **Frontend cliente:** SPA desacoplada en Angular (`firmeza-frontend`).

---

## 🛠️ Stack técnico

| Capa / Módulo | Tecnología |
| :--- | :--- |
| **API RESTful** | ASP.NET Core 10 Web API |
| **Panel Web** | ASP.NET Core 10 MVC (Razor Views + Bootstrap 5) |
| **Base de datos** | PostgreSQL 16 / Supabase + Npgsql.EntityFrameworkCore |
| **ORM** | Entity Framework Core 10 (Code First + Migraciones) |
| **Autenticación Panel** | ASP.NET Core Identity (Cookies) |
| **Autenticación API** | JWT Bearer Tokens (Roles: `Administrador`, `Cliente`) |
| **Mapeo de DTOs** | AutoMapper 15.1 |
| **Documentación API** | Swagger UI (Swashbuckle con soporte JWT) |
| **Servicio de Correo** | SMTP (Gmail) desacoplado mediante Clean Architecture |
| **Importación / Exportación Excel** | EPPlus |
| **Generación de Reportes / PDF** | QuestPDF |
| **Pruebas unitarias** | xUnit (26 pruebas automatizadas en verde) |
| **Empaquetado** | Docker + Docker Compose |
| **Frontend Cliente** | Angular (Semana 4) |

---

## 🏗️ Arquitectura (Clean Architecture)

Las dependencias fluyen en una sola dirección hacia el dominio, desacoplando completamente la lógica de negocio de la infraestructura y de las interfaces de usuario:

```
    ┌─────────────────┐       ┌─────────────────┐
    │   Firmeza.Web   │       │   Firmeza.Api   │
    │   (Panel Razor) │       │   (API REST)    │
    └────────┬────────┘       └────────┬────────┘
             │                         │
             ▼                         ▼
    ┌───────────────────────────────────────────┐
    │            Firmeza.Application            │
    │   (Interfaces, DTOs, MappingProfile)      │
    └─────────────────────┬─────────────────────┘
                          ▲
                          │
    ┌─────────────────────┴─────────────────────┐
    │           Firmeza.Infrastructure          │
    │   (DbContext, Identity, SMTP, Servicios)  │
    └─────────────────────┬─────────────────────┘
                          │
                          ▼
    ┌───────────────────────────────────────────┐
    │              Firmeza.Domain               │
    │         (Entidades puras del negocio)     │
    └───────────────────────────────────────────┘
```

* **`Firmeza.Domain`**: Entidades centrales del negocio:
  * **Vehículos & Operaciones:** `Vehiculo`, `VentaVehiculo`, `Renta`.
  * **Repuestos & Ventas:** `Producto` (Autopartes y repuestos), `Venta`, `DetalleVenta`.
  * **Clientes y Seguridad:** `Cliente`, `Roles` (`Administrador`, `Cliente`).
* **`Firmeza.Application`**: Contratos e interfaces (`IExcelImportService`, `IExportService`, `IReciboService`, `IEmailService`), DTOs estructurados y perfiles de AutoMapper.
* **`Firmeza.Infrastructure`**: Implementación de base de datos (`ApplicationDbContext`, migraciones), `DbInitializer` con datos semilla automotrices, Identity (`ApplicationUser`), y servicios externos (`ExcelImportService`, `ExportService`, `ReciboService`, `SmtpEmailService`).
* **`Firmeza.Api`**: Controladores RESTful, seguridad JWT, configuración de Swagger y CORS habilitado para el frontend.
* **`Firmeza.Web`**: Panel de administración MVC y vistas Razor.

---

## 🔐 Roles del sistema

* **`Administrador`**: Acceso total al panel Razor y permisos de administración en la API (crear/modificar repuestos, vehículos, gestionar clientes, exportaciones y supervisar ventas y rentas).
* **`Cliente`**: Usuarios registrados a través de la API (`/api/auth/registro-cliente`) para consultar vehículos, solicitar rentas o comprar repuestos desde el frontend.

---

## 🚀 API RESTful (Semana 3)

### Endpoints principales

#### 🔑 Autenticación (`/api/auth`)
* `POST /api/auth/login`: Autentica credenciales y devuelve el token JWT con sus claims y roles.
* `POST /api/auth/registro-cliente`: Registra un nuevo usuario con rol `Cliente` y dispara un correo de bienvenida automático al concesionario.

#### 🚘 Vehículos (`/api/vehiculos`)
* `GET /api/vehiculos`: Catálogo de vehículos disponibles (filtros por marca, modelo, estado).
* `GET /api/vehiculos/{id}`: Detalle técnico de un vehículo (VIN, placa, kilometraje, tarifa de renta, precio de venta).
* `POST /api/vehiculos`: Registra un nuevo vehículo al inventario (`Administrador`).
* `PUT /api/vehiculos/{id}`: Actualiza los datos de un vehículo (`Administrador`).
* `DELETE /api/vehiculos/{id}`: Da de baja un vehículo (`Administrador`).

#### 🏷️ Venta de Vehículos (`/api/ventasvehiculos`)
* `GET /api/ventasvehiculos`: Historial de ventas de vehículos.
* `POST /api/ventasvehiculos`: Registra la venta de un auto, asociando cliente, vendedor, precio final y cambiando el estado del vehículo a "Vendido".

#### ⏱️ Rentas y Alquiler de Autos (`/api/rentas`)
* `GET /api/rentas`: Historial de contratos de renta.
* `GET /api/rentas/{id}`: Detalle de renta por ID.
* `POST /api/rentas`: Registra una nueva renta calculando días, tarifa diaria, depósito de garantía y marcando el vehículo como "Rentado".
* `PUT /api/rentas/{id}/finalizar`: Finaliza la renta, calcula posibles cargos adicionales y vuelve a poner el vehículo en estado "Disponible".

#### 🔧 Repuestos y Autopartes (`/api/productos`)
* `GET /api/productos`: Catálogo de repuestos con soporte para búsqueda (`?buscar=pastillas`) y filtro por categoría (`?categoria=Frenos`).
* `GET /api/productos/{id}`: Detalle de un repuesto específico.
* `POST /api/productos`: Crea un nuevo repuesto en inventario (`Administrador`).
* `PUT /api/productos/{id}`: Actualiza precio, descripción o datos del repuesto (`Administrador`).
* `DELETE /api/productos/{id}`: Elimina un repuesto (`Administrador`).

#### 🛒 Ventas de Repuestos (`/api/ventas`)
* `GET /api/ventas`: Historial de ventas de repuestos.
* `POST /api/ventas`: Procesa una venta de repuestos con:
  * Validación de stock acumulado por repuesto.
  * Descuento automático de inventario.
  * Cálculo contable de Subtotal e IVA (19%).
  * Generación automática del recibo PDF en `wwwroot/recibos/`.
  * Envío de confirmación de compra por correo electrónico.

#### 👥 Clientes (`/api/clientes`)
* `GET /api/clientes`: Listado de clientes (`Administrador`).
* `GET /api/clientes/{id}`: Consulta de cliente por ID.
* `POST /api/clientes`: Crea un cliente con validación de edad (18 a 120 años).
* `PUT /api/clientes/{id}`: Modifica los datos de un cliente.
* `DELETE /api/clientes/{id}`: Elimina un cliente.

#### 📊 Exportación Masiva (`/api/exportacion`)
* `GET /api/exportacion/vehiculos/excel` y `/pdf`: Exporta el inventario de vehículos.
* `GET /api/exportacion/productos/excel` y `/pdf`: Exporta el listado de repuestos.
* `GET /api/exportacion/clientes/excel` y `/pdf`: Exporta la base de clientes.
* `GET /api/exportacion/ventas/excel` y `/pdf`: Exporta las ventas de repuestos.
* `GET /api/exportacion/ventas-vehiculos/excel` y `/pdf`: Exporta las ventas de vehículos.
* `GET /api/exportacion/rentas/excel` y `/pdf`: Exporta los contratos de renta.

---

## 📖 Cómo probar la API con Swagger UI

1. Ejecuta el proyecto `Firmeza.Api`:
   ```bash
   dotnet run --project src/Firmeza.Api
   ```
2. Abre en tu navegador la URL de Swagger:
   ```text
   http://localhost:5200/swagger   (o el puerto que indique la consola)
   ```
3. **Para probar endpoints protegidos:**
   * Haz una petición en `POST /api/auth/login` con el usuario administrador inicial:
     * **Email:** `admin@firmeza.com`
     * **Password:** `Admin123$`
   * Copia el valor del `"token"` recibido en la respuesta JSON.
   * Haz clic en el botón verde **Authorize** (candado 🔓) en la parte superior derecha de Swagger.
   * Pega el token y presiona **Authorize**.
   * ¡Listo! Ahora todos los endpoints con autorización funcionarán directamente desde Swagger.

---

## 📧 Configuración de Correo (SMTP Gmail)

En `src/Firmeza.Api/appsettings.json`:

```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "SenderEmail": "tu_correo@gmail.com",
  "SenderName": "Firmeza Concesionario",
  "Password": "TU_CONTRASENA_DE_APLICACION_GMAIL",
  "EnableSsl": true
}
```

> **Nota sobre Gmail:** Para enviar correos reales, debes ingresar a tu cuenta de Google → **Seguridad** → **Verificación en 2 pasos** → **Contraseñas de aplicaciones**, generar una clave de 16 letras y colocarla en `Password`. Si se deja el valor por defecto, el servicio simula el envío y registra un log sin fallar.

---

## 🧪 Pruebas Unitarias

El proyecto cuenta con **26 pruebas unitarias automatizadas** pasando en verde usando **xUnit**:

```bash
dotnet test src/Firmeza.Tests
```

### Módulos probados:
* **`EdadValidatorTests` (18 tests):** Validación de edad mínima (18) y máxima (120), valores nulos, texto no numérico y desbordamientos.
* **`VentaTests` (3 tests):** Cálculo exacto de IVA (19%), Subtotal, Total y descuento de inventario.
* **`MappingProfileTests` (4 tests):** Inicialización y validación de AutoMapper para DTOs de Vehículos, Repuestos, Clientes y Ventas.
* **`SmtpEmailServiceTests` (1 test):** Resiliencia y manejo de fallback en el servicio de correo.

---

## 🐳 Despliegue con Docker y Base de Datos

### Despliegue Orquestado (Semana 5):

El archivo `docker-compose.yml` orquesta todo el ecosistema asegurando **calidad previa**:
1. **`tests` (Paso obligatorio inicial):** Ejecuta las **26 pruebas automatizadas** de xUnit en un contenedor aislado. Si alguna falla, Docker cancela el despliegue.
2. **`db` (PostgreSQL 16):** Levanta la base de datos relacional y espera a que esté saludable (`healthcheck`).
3. **`api` (Firmeza.Api):** Levanta la API REST en modo producción.
4. **`admin` (Firmeza.Admin / MVC):** Levanta el panel administrativo.
5. **`client` (Firmeza.Client / Angular):** Compila la SPA y la sirve optimizada mediante **Nginx**.

#### Comando único de despliegue:
```bash
docker compose up --build
```

#### Puertos del sistema:
* 🌐 **Frontend Cliente (Angular):** `http://localhost:4200`
* 🔌 **API REST / Swagger:** `http://localhost:5000/swagger` (y `http://localhost:5077`)
* 🖥️ **Panel Administrativo (Razor):** `http://localhost:8080`
* 🗄️ **Base de datos (PostgreSQL):** `localhost:5432`

