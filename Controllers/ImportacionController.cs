using Firmeza.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Firmeza.Web.Services;
using Firmeza.Web.Models.ViewModels;

namespace Firmeza.Web.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class ImportacionController : Controller
    {
        private readonly IExcelImportService _importService;

        public ImportacionController(IExcelImportService importService)
        {
            _importService = importService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ImportResultViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                ModelState.AddModelError("", "Debes seleccionar un archivo .xlsx.");
                return View(new ImportResultViewModel());
            }

            if (!Path.GetExtension(archivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "El archivo debe tener extensión .xlsx.");
                return View(new ImportResultViewModel());
            }

            using var stream = archivo.OpenReadStream();
            var resultado = await _importService.ImportarAsync(stream);

            return View("Resultado", resultado);
        }
    }
}