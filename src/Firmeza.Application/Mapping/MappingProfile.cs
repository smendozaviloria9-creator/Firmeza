using AutoMapper;
using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.DTOs.Productos;
using Firmeza.Application.DTOs.Ventas;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Producto
        CreateMap<Producto, ProductoDto>();
        CreateMap<ProductoCreateDto, Producto>();
        CreateMap<ProductoUpdateDto, Producto>();

        // Cliente
        CreateMap<Cliente, ClienteDto>();
        CreateMap<ClienteCreateDto, Cliente>();
        CreateMap<ClienteUpdateDto, Cliente>();

        // Venta
        CreateMap<Venta, VentaDto>()
            .ForMember(dest => dest.ClienteNombre,
                opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nombres + " " + src.Cliente.Apellidos : string.Empty));

        CreateMap<DetalleVenta, DetalleVentaDto>()
            .ForMember(dest => dest.ProductoNombre,
                opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));
    }
}
