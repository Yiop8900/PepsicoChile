namespace PepsicoChile.Models.ViewModels
{
    public class GestionIngresoViewModel
 {
     public IngresoTaller Ingreso { get; set; } = new();
        public List<TareaTaller> Tareas { get; set; } = new();
     public List<Pausa> Pausas { get; set; } = new();
     public List<Documento> Documentos { get; set; } = new();
        public List<Usuario> MecanicosDisponibles { get; set; } = new();
   public bool PuedeEditarTareas { get; set; } = true;
        
    // Propiedades para programar ingreso
    public List<Vehiculo> VehiculosDisponibles { get; set; } = new();
     public List<Usuario> ChofersDisponibles { get; set; } = new();
        public DateTime FechaProgramada { get; set; }
    }
}
