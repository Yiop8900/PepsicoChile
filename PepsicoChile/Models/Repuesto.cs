namespace PepsicoChile.Models
{
   public class Repuesto
  {
        public int Id { get; set; }
        public int TareaTallerId { get; set; }
        public TareaTaller? TareaTaller { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CodigoRepuesto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string? Proveedor { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public string Estado { get; set; } = "Solicitado"; // Solicitado, En Tránsito, Recibido, Instalado
    }
}
