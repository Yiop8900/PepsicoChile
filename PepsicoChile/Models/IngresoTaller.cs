namespace PepsicoChile.Models
{
    public class IngresoTaller
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }
        public int ChoferId { get; set; }
        public Usuario? Chofer { get; set; }
        public DateTime FechaProgramada { get; set; }
        public DateTime? FechaIngresoReal { get; set; }
        public DateTime? FechaSalidaEstimada { get; set; }
        public DateTime? FechaSalidaReal { get; set; }
        public string MotivoIngreso { get; set; } = string.Empty; // Mantenimiento, Reparación, Siniestro
        public string Estado { get; set; } = "Programado"; // Programado, En Proceso, Pausado, Completado, Cancelado
        public string? DescripcionProblema { get; set; }
        public string? ObservacionesChofer { get; set; }
        public int? SupervisorId { get; set; }
        public Usuario? Supervisor { get; set; }
        public int? MecanicoAsignadoId { get; set; }
        public Usuario? MecanicoAsignado { get; set; }
        public bool RequiereRepuestos { get; set; }
        public int? KilometrajeIngreso { get; set; }
    }
}
