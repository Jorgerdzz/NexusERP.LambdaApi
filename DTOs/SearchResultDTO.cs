namespace ApiNexusERP.DTOs
{
    public class SearchResultDTO
    {
        public string Categoria { get; set; } // "Empleado", "Cliente", "Factura"
        public string Titulo { get; set; }    // Nombre principal
        public string Subtitulo { get; set; } // DNI, CIF, etc.
        public string Url { get; set; }       // La ruta para tu frontend (ej: "/empleados/1")
        public string Icono { get; set; }     // Clase del icono (ej: "fas fa-user", "fas fa-file-invoice")
    }
}