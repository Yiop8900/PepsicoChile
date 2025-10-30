using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La patente es obligatoria")]
        [StringLength(10)]
        public string Patente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(100)]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        [StringLength(100)]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año es obligatorio")]
        [Range(1900, 2100, ErrorMessage = "Año inválido")]
        public int Año { get; set; }

        [Required(ErrorMessage = "El tipo de vehículo es obligatorio")]
        [StringLength(50)]
        public string TipoVehiculo { get; set; } = string.Empty;

        [StringLength(50)]
        public string? NumeroFlota { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Disponible";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // Campos adicionales
        public int? KilometrajeActual { get; set; }

        [StringLength(50)]
        public string? Motor { get; set; }

        [StringLength(50)]
        public string? Chasis { get; set; }

        [StringLength(30)]
        public string? Color { get; set; }

        [StringLength(20)]
        public string? Combustible { get; set; }

        public decimal? CapacidadCarga { get; set; }
        public DateTime? UltimoMantenimiento { get; set; }
        public DateTime? ProximoMantenimiento { get; set; }
    }
}
