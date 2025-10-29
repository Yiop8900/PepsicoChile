namespace PepsicoChile.Models
{
    public class Documento
 {
        public int Id { get; set; }
        public int IngresoTallerId { get; set; }
        public IngresoTaller? IngresoTaller { get; set; }
        public string TipoDocumento { get; set; } = string.Empty; // Foto, Informe, Presupuesto, Orden de Trabajo
        public string NombreArchivo { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
        public int? UsuarioSubidaId { get; set; }
        public Usuario? UsuarioSubida { get; set; }
        public string? Descripcion { get; set; }
        public long TamañoBytes { get; set; }
    }
}
