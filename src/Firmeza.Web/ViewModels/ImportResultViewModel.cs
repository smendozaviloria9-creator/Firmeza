namespace Firmeza.Web.ViewModels
{
    public class ImportResultViewModel
    {
        public int ClientesInsertados { get; set; }
        public int ClientesActualizados { get; set; }
        public int ProductosInsertados { get; set; }
        public int ProductosActualizados { get; set; }
        public int VentasInsertadas { get; set; }
        public List<string> Errores { get; set; } = new();
        public bool TuvoErrores => Errores.Count > 0;
    }
}