using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models.ViewModels
{
    public class GestionarIngresoViewModel
    {
        public IngresoTaller Ingreso { get; set; } = new();
        public List<TareaTaller> Tareas { get; set; } = new();
        public List<Pausa> Pausas { get; set; } = new();
        public List<Documento> Documentos { get; set; } = new();
        public List<Usuario> MecanicosDisponibles { get; set; } = new();
        public List<Repuesto> RepuestosUtilizados { get; set; } = new();
        public bool PuedeEditarTareas { get; set; } = true;

        // Para crear nueva tarea
        [Display(Name = "Descripción de la Tarea")]
        public string? NuevaTareaDescripcion { get; set; }

        [Display(Name = "Mecánico Asignado")]
        public int? NuevaTareaMecanicoId { get; set; }

        [Display(Name = "Prioridad")]
        public string? NuevaTareaPrioridad { get; set; }

        // Para registrar pausa
        [Display(Name = "Motivo de la Pausa")]
        public string? PausaMotivo { get; set; }

        [Display(Name = "Descripción")]
        public string? PausaDescripcion { get; set; }

        // Para finalizar ingreso
        [Display(Name = "Observaciones Finales")]
        public string? ObservacionesFinales { get; set; }

        [Display(Name = "Kilometraje de Salida")]
        public int? KilometrajeSalida { get; set; }
    }
}
