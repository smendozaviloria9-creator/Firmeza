using AutoMapper;
using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.DTOs.Productos;
using Firmeza.Application.DTOs.Rentas;
using Firmeza.Application.DTOs.Vehiculos;
using Firmeza.Application.DTOs.Ventas;
using Firmeza.Application.DTOs.VentasVehiculos;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Producto (repuestos)
        CreateMap<Producto, ProductoDto>();
        CreateMap<ProductoCreateDto, Producto>();
        CreateMap<ProductoUpdateDto, Producto>();

        // Cliente
        CreateMap<Cliente, ClienteDto>();
        CreateMap<ClienteCreateDto, Cliente>();
        CreateMap<ClienteUpdateDto, Cliente>();

        // Venta de repuestos
        CreateMap<Venta, VentaDto>()
            .ForMember(dest => dest.ClienteNombre,
                opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nombres + " " + src.Cliente.Apellidos : string.Empty));

        CreateMap<DetalleVenta, DetalleVentaDto>()
            .ForMember(dest => dest.ProductoNombre,
                opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));

        // Vehiculo
        CreateMap<Vehiculo, VehiculoDto>();
        CreateMap<VehiculoCreateDto, Vehiculo>();
        CreateMap<VehiculoUpdateDto, Vehiculo>();

        // Venta de vehiculo
        CreateMap<VentaVehiculo, VentaVehiculoDto>()
            .ForMember(dest => dest.ClienteNombre,
                opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nombres + " " + src.Cliente.Apellidos : string.Empty))
            .ForMember(dest => dest.VehiculoDescripcion,
                opt => opt.MapFrom(src => src.Vehiculo != null ? src.Vehiculo.Marca + " " + src.Vehiculo.Modelo + " " + src.Vehiculo.Anio.ToString() + " (" + src.Vehiculo.Placa + ")" : string.Empty));

        // Renta
        CreateMap<Renta, RentaDto>()
            .ForMember(dest => dest.ClienteNombre,
                opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nombres + " " + src.Cliente.Apellidos : string.Empty))
            .ForMember(dest => dest.VehiculoDescripcion,
                opt => opt.MapFrom(src => src.Vehiculo != null ? src.Vehiculo.Marca + " " + src.Vehiculo.Modelo + " " + src.Vehiculo.Anio.ToString() + " (" + src.Vehiculo.Placa + ")" : string.Empty));
    }
}
