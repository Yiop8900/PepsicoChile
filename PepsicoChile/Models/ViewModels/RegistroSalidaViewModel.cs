using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models.ViewModels
{
    public class RegistroSalidaViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un ingreso")]
        [Display(Name = "Vehículo en Taller")]
        public int IngresoId { get; set; }

        [Required(ErrorMessage = "El kilometraje de salida es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser un número positivo")]
        [Display(Name = "Kilometraje de Salida")]
        public int KilometrajeSalida { get; set; }

        [Display(Name = "Observaciones de Salida")]
        [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
        public string? ObservacionesSalida { get; set; }

        [Display(Name = "Imágenes de Salida")]
        public List<IFormFile>? ImagenesSalida { get; set; }

        // Lista de ingresos disponibles para salir
        public List<IngresoDisponibleSalida> IngresosDisponibles { get; set; } = new();
    }

    public class IngresoDisponibleSalida
    {
        public int Id { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string VehiculoInfo { get; set; } = string.Empty; // Marca + Modelo
        public string ChoferNombre { get; set; } = string.Empty;
        public DateTime FechaLlegada { get; set; }
        public int KilometrajeIngreso { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string EstadoColor { get; set; } = string.Empty;
        public string NumeroOT { get; set; } = string.Empty;
        public int TiempoEnTaller { get; set; } // En horas
    }
}
