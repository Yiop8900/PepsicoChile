namespace PepsicoChile.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Información del usuario
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;

        // Estadísticas generales
        public int VehiculosEnTaller { get; set; }
        public int VehiculosDisponibles { get; set; }
        public int VehiculosProgramados { get; set; }
        public int IngresosEnProceso { get; set; }
        
        // Tareas
        public int TareasPendientes { get; set; }
        public int TareasEnProceso { get; set; }
        public int TareasCompletadasHoy { get; set; }
        
        // Notificaciones y alertas
        public int NotificacionesNoLeidas { get; set; }
        public int PausasActivas { get; set; }

        // Listas de datos
        public List<IngresoTaller> IngresosRecientes { get; set; } = new();
        public List<TareaTaller> TareasUrgentes { get; set; } = new();
    }
}
