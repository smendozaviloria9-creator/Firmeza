using Firmeza.Application.DTOs;

namespace Firmeza.Application.Interfaces;

public interface IExcelImportService
{
    Task<ImportResultViewModel> ImportarAsync(Stream archivoStream);
}
