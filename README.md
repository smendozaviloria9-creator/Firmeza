# Firmeza — Sistema de gestión de materiales de construcción

Sistema integral para una empresa de venta de materiales de construcción con arquitectura limpia (**Clean Architecture**) en 4 capas, que incluye:
* **Panel administrativo Razor MVC** (`Firmeza.Web`) con autenticación por roles, importación/exportación masiva Excel y recibos PDF.
* **API RESTful** (`Firmeza.Api`) con autenticación JWT, AutoMapper, documentación Swagger interactiva y servicio de notificaciones por correo SMTP.
* **Base de datos compartida** en PostgreSQL 16 con Entity Framework Core 10.
* **Frontend cliente** en Angular (`firmeza-frontend`).

---

## 🛠️ Stack técnico

| Capa / Módulo | Tecnología |
| :--- | :--- |
| **API RESTful** | ASP.NET Core 10 Web API |
| **Panel Web** | ASP.NET Core 10 MVC (Razor Views + Bootstrap 5) |
| **Base de datos** | PostgreSQL 16 + Npgsql.EntityFrameworkCore |
| **ORM** | Entity Framework Core 10 (Code First + Migraciones) |
| **Autenticación Panel** | ASP.NET Core Identity (Cookies) |
| **Autenticación API** | JWT Bearer Tokens (Roles: `Administrador`, `Cliente`) |
| **Mapeo de DTOs** | AutoMapper 15.1 |
| **Documentación API** | Swagger UI (Swashbuckle con soporte JWT) |
| **Servicio de Correo** | SMTP (Gmail) desacoplado mediante Clean Architecture |
| **Importación/Excel** | EPPlus |
| **Exportación/PDF** | QuestPDF |
| **Pruebas unitarias** | xUnit (26 pruebas automatizadas) |
| **Empaquetado** | Docker + Docker Compose |
| **Frontend Cliente** | Angular (en desarrollo) |

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

* **`Firmeza.Domain`**: Entidades del negocio sin dependencias externas (`Producto`, `Cliente`, `Venta`, `DetalleVenta`, `Roles`).
* **`Firmeza.Application`**: Contratos e interfaces (`IExcelImportService`, `IExportService`, `IReciboService`, `IEmailService`), DTOs y perfiles de AutoMapper.
* **`Firmeza.Infrastructure`**: Implementación de base de datos (`ApplicationDbContext`, migraciones), Identity (`ApplicationUser`), servicios externos (`ExcelImportService`, `ExportService`, `ReciboService`, `SmtpEmailService`).
* **`Firmeza.Api`**: Controladores RESTful, seguridad JWT, configuración de Swagger y CORS.
* **`Firmeza.Web`**: Controladores MVC y vistas Razor para el panel de administración.

---

## 🔐 Roles del sistema

* **`Administrador`**: Acceso total al panel Razor y permisos de escritura/eliminación en la API (crear/modificar productos, gestionar clientes y registrar ventas).
* **`Cliente`**: Usuarios registrados a través de la API (`/api/auth/registro-cliente`) para realizar pedidos y compras desde aplicaciones cliente (como Angular o Blazor).

---

## 🚀 API RESTful (Semana 3)

### Endpoints principales

#### 🔑 Autenticación (`/api/auth`)
* `POST /api/auth/login`: Autentica credenciales y devuelve el token JWT con sus claims y roles.
* `POST /api/auth/registro-cliente`: Registra un nuevo usuario con rol `Cliente` y dispara un correo de bienvenida automático.

#### 📦 Productos (`/api/productos`)
* `GET /api/productos`: Lista de productos con soporte para búsqueda (`?buscar=cemento`) y filtro por categoría (`?categoria=Materiales`).
* `GET /api/productos/{id}`: Detalle de un producto por su identificador.
* `POST /api/productos`: Crea un nuevo producto (Requiere rol `Administrador`).
* `PUT /api/productos/{id}`: Actualiza un producto existente (Requiere rol `Administrador`).
* `DELETE /api/productos/{id}`: Elimina un producto (Requiere rol `Administrador`).

#### 👥 Clientes (`/api/clientes`)
* `GET /api/clientes`: Listado de clientes (Requiere rol `Administrador`).
* `GET /api/clientes/{id}`: Detalle de cliente por ID.
* `POST /api/clientes`: Crea un cliente con validación de edad (18-120 años).
* `PUT /api/clientes/{id}`: Modifica datos de un cliente.
* `DELETE /api/clientes/{id}`: Elimina un cliente.

#### 🛒 Ventas (`/api/ventas`)
* `GET /api/ventas`: Historial de ventas con sus clientes y productos incluidos.
* `GET /api/ventas/{id}`: Detalle completo de una venta.
* `POST /api/ventas`: Registra una nueva venta con:
  * Validación de stock acumulado por producto (previene stock negativo).
  * Descuento automático de inventario.
  * Cálculo contable de Subtotal e IVA (19%).
  * Generación automática del recibo PDF en `wwwroot/recibos/`.
  * Envío de correo de confirmación de compra al cliente.
* `DELETE /api/ventas/{id}`: Elimina una venta (Requiere rol `Administrador`).

---

## 📖 Cómo probar la API con Swagger UI

1. Ejecuta el proyecto `Firmeza.Api`:
   ```bash
   cd src/Firmeza.Api
   dotnet run
   ```
2. Abre en tu navegador la URL de Swagger:
   ```text
   http://localhost:5000/swagger   (o el puerto asignado por la consola)
   ```
3. **Para probar endpoints protegidos:**
   * Haz una petición en `POST /api/auth/login` con el usuario administrador inicial:
     * **Email:** `admin@firmeza.com`
     * **Password:** `Admin123$`
   * Copia el valor de `"token"` recibido en la respuesta JSON.
   * Haz clic en el botón verde **Authorize** (candado 🔓) en la parte superior derecha de Swagger.
   * Pega el token y presiona **Authorize**.
   * ¡Listo! Ahora todos los endpoints con autorización funcionarán directamente desde Swagger.

---

## 📧 Configuración del servicio de correo (SMTP Gmail)

En `src/Firmeza.Api/appsettings.json`:

```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "SenderEmail": "tu_correo@gmail.com",
  "SenderName": "Firmeza Materiales",
  "Password": "TU_CONTRASENA_DE_APLICACION_GMAIL",
  "EnableSsl": true
}
```

> **Nota sobre Gmail:** Para enviar correos reales, debes ingresar a tu cuenta de Google → **Seguridad** → **Verificación en 2 pasos** → **Contraseñas de aplicaciones**, generar una clave de 16 letras y colocarla en `Password`. Si se deja el valor por defecto, el servicio simula el envío y registra un log sin fallar.

---

## 🧪 Pruebas unitarias

El proyecto cuenta con **26 pruebas unitarias automatizadas** usando **xUnit**:

```bash
cd src/Firmeza.Tests
dotnet test
```

### Módulos probados:
* **`EdadValidatorTests` (18 tests):** Validación de edad mínima (18) y máxima (120), valores nulos, texto no numérico y desbordamientos.
* **`VentaTests` (3 tests):** Cálculo exacto de IVA (19%), Subtotal, Total y descuento de inventario.
* **`MappingProfileTests` (4 tests):** Inicialización y validación de AutoMapper para DTOs de Productos, Clientes y Ventas.
* **`SmtpEmailServiceTests` (1 test):** Resiliencia y manejo de fallback en el servicio de correo.

---

## 🐳 Despliegue con Docker

Crea un archivo `.env` en la raíz con la contraseña de PostgreSQL:
```bash
echo "DB_PASSWORD=TU_CLAVE" > .env
```

Levanta los contenedores:
```bash
docker compose up -d --build
```
* **PostgreSQL:** `localhost:5432`
* **Panel Web:** `http://localhost:8080`
