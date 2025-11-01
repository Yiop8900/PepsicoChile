using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models.ViewModels
{
    public class ProgramarIngresoViewModel
{
        [Required(ErrorMessage = "Debe seleccionar un vehículo")]
        [Display(Name = "Vehículo")]
        public int VehiculoId { get; set; }

   [Required(ErrorMessage = "Debe seleccionar un chofer")]
  [Display(Name = "Chofer Responsable")]
        public int ChoferId { get; set; }

      [Display(Name = "Mecánico Asignado")]
        public int? MecanicoAsignadoId { get; set; }

      [Required(ErrorMessage = "La fecha programada es obligatoria")]
      [Display(Name = "Fecha y Hora Programada")]
        [DataType(DataType.DateTime)]
        public DateTime FechaProgramada { get; set; }

        [Required(ErrorMessage = "Debe especificar el motivo del ingreso")]
    [Display(Name = "Motivo del Ingreso")]
        public string MotivoIngreso { get; set; } = string.Empty;

    [Display(Name = "Descripción del Problema")]
        [DataType(DataType.MultilineText)]
     public string? DescripcionProblema { get; set; }

    [Display(Name = "Requiere Repuestos")]
 public bool RequiereRepuestos { get; set; }

   [Display(Name = "Kilometraje Actual")]
        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser positivo")]
      public int? KilometrajeIngreso { get; set; }

        [Display(Name = "Fecha Estimada de Salida")]
 [DataType(DataType.Date)]
        public DateTime? FechaSalidaEstimada { get; set; }

      [Display(Name = "Observaciones del Chofer")]
        [DataType(DataType.MultilineText)]
        public string? ObservacionesChofer { get; set; }

        // Listas para dropdowns
public List<Vehiculo> VehiculosDisponibles { get; set; } = new();
      public List<Usuario> ChofersDisponibles { get; set; } = new();
        public List<Usuario> MecanicosDisponibles { get; set; } = new();
    }
}
