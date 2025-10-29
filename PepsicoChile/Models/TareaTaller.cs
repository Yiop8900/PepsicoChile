namespace PepsicoChile.Models
{
    public class TareaTaller
    {
        public int Id { get; set; }
        public int IngresoTallerId { get; set; }
        public IngresoTaller? IngresoTaller { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int? MecanicoAsignadoId { get; set; }
        public Usuario? MecanicoAsignado { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, En Proceso, Completada, Cancelada
        public string? Observaciones { get; set; }
        public int TiempoEstimadoHoras { get; set; }
        public int? TiempoRealHoras { get; set; }
        public string Prioridad { get; set; } = "Media"; // Alta, Media, Baja
    }
}
