namespace PepsicoChile.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;
        public int VehiculosEnTaller { get; set; }
        public int VehiculosProgramados { get; set; }
        public int TareasPendientes { get; set; }
        public int TareasEnProceso { get; set; }
        public List<IngresoTaller> IngresosRecientes { get; set; } = new();
        public List<TareaTaller> TareasUrgentes { get; set; } = new();
  }
}
