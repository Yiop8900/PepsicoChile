using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models.ViewModels
{
    public class RegistroIngresoViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un vehículo")]
        public int VehiculoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un chofer")]
        public int ChoferId { get; set; }

        [Required(ErrorMessage = "La fecha programada es requerida")]
        [Display(Name = "Fecha Programada")]
        public DateTime FechaProgramada { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El motivo de ingreso es requerido")]
        [Display(Name = "Motivo de Ingreso")]
        public string MotivoIngreso { get; set; } = string.Empty;

        [Display(Name = "Descripción del Problema")]
        public string? DescripcionProblema { get; set; }

        [Display(Name = "Observaciones del Chofer")]
        public string? ObservacionesChofer { get; set; }

        [Required(ErrorMessage = "El kilometraje es requerido")]
        [Display(Name = "Kilometraje Actual")]
        public int KilometrajeIngreso { get; set; }

        [Display(Name = "Requiere Repuestos")]
        public bool RequiereRepuestos { get; set; }

        [Display(Name = "Número de OT")]
        public string NumeroOT { get; set; } = string.Empty;

        // Imágenes del vehículo
        [Display(Name = "Imágenes del Vehículo")]
        public List<IFormFile>? ImagenesVehiculo { get; set; }

        // Para dropdowns
        public List<Vehiculo> VehiculosDisponibles { get; set; } = new();
        public List<Usuario> ChofersDisponibles { get; set; } = new();
    }
}
