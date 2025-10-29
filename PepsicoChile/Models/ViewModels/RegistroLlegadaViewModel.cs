namespace PepsicoChile.Models.ViewModels
{
    public class RegistroLlegadaViewModel
    {
        public int VehiculoId { get; set; }
        public string Patente { get; set; } = string.Empty;
        public int ChoferId { get; set; }
        public string NombreChofer { get; set; } = string.Empty;
        public DateTime FechaHoraLlegada { get; set; } = DateTime.Now;
        public string MotivoIngreso { get; set; } = string.Empty;
        public string DescripcionProblema { get; set; } = string.Empty;
        public string? ObservacionesChofer { get; set; }
        public int KilometrajeIngreso { get; set; }
        public bool RequiereRepuestos { get; set; }
        public List<Vehiculo> VehiculosDisponibles { get; set; } = new();
    }
}
