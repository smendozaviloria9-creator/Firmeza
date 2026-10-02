using AutoMapper;
using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.DTOs.Productos;
using Firmeza.Application.DTOs.Ventas;
using Firmeza.Application.Mapping;
using Firmeza.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Firmeza.Tests;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void AutoMapper_SeInicializaCorrectamente()
    {
        Assert.NotNull(_mapper);
    }

    [Fact]
    public void Mapear_ProductoCreateDto_A_Producto_Correctamente()
    {
        // Arrange
        var dto = new ProductoCreateDto
        {
            Nombre = "Cemento Portland",
            Descripcion = "Bulto de 50kg",
            Categoria = "Cementos",
            UnidadMedida = "Bulto",
            PrecioUnitario = 32000m,
            Stock = 100
        };

        // Act
        var producto = _mapper.Map<Producto>(dto);

        // Assert
        Assert.NotNull(producto);
        Assert.Equal(dto.Nombre, producto.Nombre);
        Assert.Equal(dto.PrecioUnitario, producto.PrecioUnitario);
        Assert.Equal(dto.Stock, producto.Stock);
        Assert.True(producto.Activo);
    }

    [Fact]
    public void Mapear_Cliente_A_ClienteDto_Correctamente()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = 5,
            Nombres = "Carlos",
            Apellidos = "Rodriguez",
            Documento = "12345678",
            Correo = "carlos@example.com",
            Telefono = "3001234567",
            Edad = 30
        };

        // Act
        var dto = _mapper.Map<ClienteDto>(cliente);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(cliente.Id, dto.Id);
        Assert.Equal(cliente.Nombres, dto.Nombres);
        Assert.Equal(cliente.Apellidos, dto.Apellidos);
        Assert.Equal(cliente.Documento, dto.Documento);
        Assert.Equal(cliente.Correo, dto.Correo);
    }

    [Fact]
    public void Mapear_Venta_A_VentaDto_ConDetalles_Correctamente()
    {
        // Arrange
        var venta = new Venta
        {
            Id = 10,
            ClienteId = 3,
            Fecha = DateTime.UtcNow,
            Subtotal = 100000m,
            Iva = 19000m,
            Total = 119000m,
            Cliente = new Cliente
            {
                Id = 3,
                Nombres = "Ana",
                Apellidos = "Gomez",
                Documento = "98765432",
                Correo = "ana@example.com",
                Telefono = "3109876543"
            },
            Detalles = new List<DetalleVenta>
            {
                new DetalleVenta
                {
                    Id = 1,
                    ProductoId = 2,
                    Cantidad = 4,
                    PrecioUnitario = 25000m,
                    Subtotal = 100000m,
                    Producto = new Producto
                    {
                        Id = 2,
                        Nombre = "Arena Fina",
                        Categoria = "Agregados",
                        UnidadMedida = "m3",
                        PrecioUnitario = 25000m
                    }
                }
            }
        };

        // Act
        var dto = _mapper.Map<VentaDto>(venta);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(venta.Id, dto.Id);
        Assert.Equal(venta.Total, dto.Total);
        Assert.Equal("Ana Gomez", dto.ClienteNombre);
        Assert.Single(dto.Detalles);
        Assert.Equal("Arena Fina", dto.Detalles[0].ProductoNombre);
    }
}
