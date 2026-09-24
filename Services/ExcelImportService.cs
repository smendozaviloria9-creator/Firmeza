using OfficeOpenXml;
using Firmeza.Web.Data;
using Firmeza.Web.Models;
using Firmeza.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Services
{
    public interface IExcelImportService
    {
        Task<ImportResultViewModel> ImportarAsync(Stream archivoStream);
    }

    public class ExcelImportService : IExcelImportService
    {
        private readonly ApplicationDbContext _context;

        public ExcelImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResultViewModel> ImportarAsync(Stream archivoStream)
        {
            var resultado = new ImportResultViewModel();

            using var package = new ExcelPackage(archivoStream);
            var hoja = package.Workbook.Worksheets.FirstOrDefault();

            if (hoja == null || hoja.Dimension == null)
            {
                resultado.Errores.Add("El archivo está vacío o no tiene una hoja válida.");
                return resultado;
            }

            var encabezados = new Dictionary<string, int>();
            int totalColumnas = hoja.Dimension.End.Column;
            for (int col = 1; col <= totalColumnas; col++)
            {
                var nombre = hoja.Cells[1, col].Text?.Trim().ToLowerInvariant().Replace(" ", "");
                if (!string.IsNullOrEmpty(nombre) && !encabezados.ContainsKey(nombre))
                    encabezados[nombre] = col;
            }

            string? Get(int fila, string columna) =>
                encabezados.TryGetValue(columna, out var col)
                    ? hoja.Cells[fila, col].Text?.Trim()
                    : null;

            int totalFilas = hoja.Dimension.End.Row;

            for (int fila = 2; fila <= totalFilas; fila++)
            {
                var filaVacia = true;
                for (int col = 1; col <= totalColumnas; col++)
                {
                    if (!string.IsNullOrWhiteSpace(hoja.Cells[fila, col].Text))
                    {
                        filaVacia = false;
                        break;
                    }
                }
                if (filaVacia) continue;

                try
                {
                    Cliente? cliente = null;
                    Producto? producto = null;

                    var clienteNombres = Get(fila, "clientenombres");
                    var clienteApellidos = Get(fila, "clienteapellidos");
                    var clienteDocumento = Get(fila, "clientedocumento");
                    var clienteCorreo = Get(fila, "clientecorreo");
                    var clienteTelefono = Get(fila, "clientetelefono");
                    var clienteDireccion = Get(fila, "clientedireccion");
                    var clienteEdadTxt = Get(fila, "clienteedad");

                    if (!string.IsNullOrWhiteSpace(clienteDocumento))
                    {
                        if (string.IsNullOrWhiteSpace(clienteNombres) ||
                            string.IsNullOrWhiteSpace(clienteApellidos) ||
                            string.IsNullOrWhiteSpace(clienteCorreo) ||
                            string.IsNullOrWhiteSpace(clienteTelefono))
                        {
                            resultado.Errores.Add($"Fila {fila}: faltan datos obligatorios del cliente (nombres, apellidos, correo o teléfono).");
                        }
                        else if (!int.TryParse(clienteEdadTxt, out var edad) || edad < 18 || edad > 120)
                        {
                            resultado.Errores.Add($"Fila {fila}: edad de cliente inválida ('{clienteEdadTxt}'), debe estar entre 18 y 120.");
                        }
                        else
                        {
                            cliente = await _context.Clientes
                                .FirstOrDefaultAsync(c => c.Documento == clienteDocumento);

                            if (cliente == null)
                            {
                                cliente = new Cliente
                                {
                                    Nombres = clienteNombres,
                                    Apellidos = clienteApellidos,
                                    Documento = clienteDocumento,
                                    Correo = clienteCorreo,
                                    Telefono = clienteTelefono,
                                    Direccion = clienteDireccion,
                                    Edad = edad
                                };
                                _context.Clientes.Add(cliente);
                                resultado.ClientesInsertados++;
                            }
                            else
                            {
                                cliente.Nombres = clienteNombres;
                                cliente.Apellidos = clienteApellidos;
                                cliente.Correo = clienteCorreo;
                                cliente.Telefono = clienteTelefono;
                                cliente.Direccion = clienteDireccion;
                                cliente.Edad = edad;
                                resultado.ClientesActualizados++;
                            }
                        }
                    }

                    var productoNombre = Get(fila, "productonombre");
                    var productoDescripcion = Get(fila, "productodescripcion");
                    var productoCategoria = Get(fila, "productocategoria");
                    var productoUnidadMedida = Get(fila, "productounidadmedida");
                    var productoPrecioTxt = Get(fila, "productoprecio");
                    var productoStockTxt = Get(fila, "productostock");

                    if (!string.IsNullOrWhiteSpace(productoNombre))
                    {
                        if (string.IsNullOrWhiteSpace(productoCategoria) ||
                            string.IsNullOrWhiteSpace(productoUnidadMedida))
                        {
                            resultado.Errores.Add($"Fila {fila}: faltan datos obligatorios del producto (categoría o unidad de medida).");
                        }
                        else if (!decimal.TryParse(productoPrecioTxt, out var precio) || precio < 0)
                        {
                            resultado.Errores.Add($"Fila {fila}: precio de producto inválido ('{productoPrecioTxt}').");
                        }
                        else
                        {
                            int.TryParse(productoStockTxt, out var stock);

                            producto = await _context.Productos
                                .FirstOrDefaultAsync(p => p.Nombre == productoNombre);

                            if (producto == null)
                            {
                                producto = new Producto
                                {
                                    Nombre = productoNombre,
                                    Descripcion = productoDescripcion,
                                    Categoria = productoCategoria,
                                    UnidadMedida = productoUnidadMedida,
                                    PrecioUnitario = precio,
                                    Stock = stock
                                };
                                _context.Productos.Add(producto);
                                resultado.ProductosInsertados++;
                            }
                            else
                            {
                                producto.Descripcion = productoDescripcion;
                                producto.Categoria = productoCategoria;
                                producto.UnidadMedida = productoUnidadMedida;
                                producto.PrecioUnitario = precio;
                                producto.Stock = stock;
                                resultado.ProductosActualizados++;
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    var ventaCantidadTxt = Get(fila, "ventacantidad");
                    var ventaFechaTxt = Get(fila, "ventafecha");

                    if (!string.IsNullOrWhiteSpace(ventaCantidadTxt))
                    {
                        if (cliente == null || producto == null)
                        {
                            resultado.Errores.Add($"Fila {fila}: no se puede registrar la venta sin cliente y producto válidos.");
                            continue;
                        }

                        if (!int.TryParse(ventaCantidadTxt, out var cantidad) || cantidad <= 0)
                        {
                            resultado.Errores.Add($"Fila {fila}: cantidad de venta inválida ('{ventaCantidadTxt}').");
                            continue;
                        }

                        if (!DateTime.TryParse(ventaFechaTxt, out var fecha))
                            fecha = DateTime.UtcNow;

                        var subtotal = cantidad * producto.PrecioUnitario;

                        var venta = new Venta
                        {
                            ClienteId = cliente.Id,
                            Fecha = fecha,
                            Total = subtotal,
                            Detalles = new List<DetalleVenta>
                            {
                                new DetalleVenta
                                {
                                    ProductoId = producto.Id,
                                    Cantidad = cantidad,
                                    PrecioUnitario = producto.PrecioUnitario,
                                    Subtotal = subtotal
                                }
                            }
                        };

                        _context.Ventas.Add(venta);
                        resultado.VentasInsertadas++;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    resultado.Errores.Add($"Fila {fila}: error inesperado — {ex.Message}");
                }
            }

            return resultado;
        }
    }
}