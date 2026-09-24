using Firmeza.Domain.Entities;

namespace Firmeza.Application.Interfaces;

public interface IReciboService
{
    string GenerarRecibo(Venta venta, string carpetaDestino);
}
