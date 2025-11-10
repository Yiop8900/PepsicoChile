using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models
{
    public class AsignacionVehiculo
    {
        public int Id { get; set; }

        [Required]
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        [Required]
        public int ChoferId { get; set; }
        public Usuario? Chofer { get; set; }

        [Required]
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        public DateTime? FechaDesasignacion { get; set; }

        [Required]
        public bool Activa { get; set; } = true;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public int? AsignadoPorId { get; set; }
        public Usuario? AsignadoPor { get; set; }
    }
}
