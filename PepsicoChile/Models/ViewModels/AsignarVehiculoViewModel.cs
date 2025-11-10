using System.ComponentModel.DataAnnotations;
using PepsicoChile.Models;

namespace PepsicoChile.Models.ViewModels
{
    public class AsignarVehiculoViewModel
    {
        public int VehiculoId { get; set; }
        
        [Display(Name = "Patente")]
        public string Patente { get; set; } = string.Empty;
        
        [Display(Name = "Vehículo")]
        public string MarcaModelo { get; set; } = string.Empty;

        // Asignación actual
        public int? ChoferActualId { get; set; }
        public string? ChoferActualNombre { get; set; }

        // Nueva asignación
        [Required(ErrorMessage = "Debe seleccionar un chofer")]
        [Display(Name = "Chofer")]
        public int ChoferSeleccionadoId { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Lista de choferes disponibles
        public List<Usuario> Choferes { get; set; } = new();
    }
}
