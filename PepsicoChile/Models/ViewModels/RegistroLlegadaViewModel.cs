using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models.ViewModels
{
    public class RegistroLlegadaViewModel
    {
        public int VehiculoId { get; set; }
        public string Patente { get; set; } = string.Empty;
        public int ChoferId { get; set; }
        public string NombreChofer { get; set; } = string.Empty;
        
        [Display(Name = "Fecha y Hora de Llegada")]
        public DateTime FechaHoraLlegada { get; set; } = DateTime.Now;
        
        public string MotivoIngreso { get; set; } = string.Empty;
        public string DescripcionProblema { get; set; } = string.Empty;
        
        [Display(Name = "Observaciones del Guardia")]
        public string? ObservacionesChofer { get; set; }
        
        [Display(Name = "Kilometraje Actual")]
        public int KilometrajeIngreso { get; set; }
        
        public bool RequiereRepuestos { get; set; }
        
        // Imágenes del vehículo
        [Display(Name = "Imágenes del Vehículo")]
        public List<IFormFile>? ImagenesVehiculo { get; set; }
        
        public List<Vehiculo> VehiculosDisponibles { get; set; } = new();
    }
}
