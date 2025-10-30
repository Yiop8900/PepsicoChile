namespace PepsicoChile.Models.ViewModels
{
    public class ProgramarIngresoViewModel
    {
        public int VehiculoId { get; set; }
      public int ChoferId { get; set; }
     public DateTime FechaProgramada { get; set; }
 public string MotivoIngreso { get; set; } = string.Empty;
     public string DescripcionProblema { get; set; } = string.Empty;
    public bool RequiereRepuestos { get; set; }

        public List<Vehiculo> VehiculosDisponibles { get; set; } = new();
        public List<Usuario> ChofersDisponibles { get; set; } = new();
}
}
