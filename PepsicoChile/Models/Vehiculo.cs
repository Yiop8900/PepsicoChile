namespace PepsicoChile.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Año { get; set; }
        public string TipoVehiculo { get; set; } = string.Empty; // Camión, Camioneta, etc.
        public string Estado { get; set; } = "Disponible"; // Disponible, En Taller, En Ruta
        public int? KilometrajeActual { get; set; }
  }
}
