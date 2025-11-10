using PepsicoChile.Models;

namespace PepsicoChile.Models.ViewModels
{
    public class ProductividadViewModel
    {
        // Totales
        public int TareasCompletadas { get; set; }
        public int TareasEnProceso { get; set; }
        public int TareasPendientes { get; set; }
        public int TotalTareas { get; set; }

        // Eficiencia
        public double EficienciaPromedio { get; set; }
        public double TiempoPromedioCompletacion { get; set; }

        // Por mecánico
        public List<ProductividadMecanicoDto> ProductividadMecanicos { get; set; } = new();

        // Por prioridad
        public List<TareaPorPrioridadDto> TareasPorPrioridad { get; set; } = new();

        // Tareas recientes
        public List<TareaTaller> TareasRecientes { get; set; } = new();

        // Estadísticas adicionales
        public string MecanicoMasProductivo { get; set; } = string.Empty;
        public int TareasAltaPrioridad { get; set; }
        public int TareasAltaPrioridadCompletadas { get; set; }
    }

    public class ProductividadMecanicoDto
    {
        public int MecanicoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int TareasCompletadas { get; set; }
        public int TareasEnProceso { get; set; }
        public int TareasPendientes { get; set; }
        public int TareasTotal { get; set; }
        public double PorcentajeEficiencia { get; set; }
    }

    public class TareaPorPrioridadDto
    {
        public string Prioridad { get; set; } = string.Empty;
        public int Completadas { get; set; }
        public int EnProceso { get; set; }
        public int Pendientes { get; set; }
        public int Total { get; set; }
    }
}
