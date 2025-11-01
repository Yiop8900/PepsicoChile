using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models
{
    public class Notificacion
    {
    public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

 [Required]
  [StringLength(100)]
      public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Mensaje { get; set; } = string.Empty;

   [StringLength(50)]
        public string Tipo { get; set; } = "Info"; // Info, Tarea, Urgente, Sistema

        public bool Leida { get; set; } = false;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

      public DateTime? FechaLeida { get; set; }

      // Información adicional opcional
        public int? TareaId { get; set; }
        public int? IngresoId { get; set; }
    public int? VehiculoId { get; set; }

  [StringLength(200)]
        public string? UrlAccion { get; set; }

     // WhatsApp
        public bool EnviadoWhatsApp { get; set; } = false;
     public DateTime? FechaEnvioWhatsApp { get; set; }
        public string? WhatsAppSid { get; set; } // ID del mensaje de Twilio
    }
}
