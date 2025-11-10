using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models
{
    public class ImagenIngreso
    {
        public int Id { get; set; }

        [Required]
        public int IngresoTallerId { get; set; }
        public IngresoTaller? IngresoTaller { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string RutaArchivo { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public DateTime FechaSubida { get; set; } = DateTime.Now;

        public int? UsuarioSubidaId { get; set; }
        public Usuario? UsuarioSubida { get; set; }

        public long TamañoBytes { get; set; }

        [StringLength(50)]
        public string TipoImagen { get; set; } = "General"; // General, Frontal, Lateral, Posterior, Daño, Otro
    }
}
