using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models
{
    public class SolicitudRepuesto
    {
        public int Id { get; set; }

        [Required]
        public int TareaTallerId { get; set; }
        public TareaTaller? TareaTaller { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CodigoRepuesto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [StringLength(100)]
        public string? Proveedor { get; set; }

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        public DateTime? FechaEntrega { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Solicitado"; // Solicitado, En Tránsito, Entregado, Instalado

        public int? SolicitadoPorId { get; set; }
        public Usuario? SolicitadoPor { get; set; }
    }
}
