namespace PepsicoChile.Models.ViewModels
{
    public class TiempoProcesoViewModel
    {
        public int TotalIngresos { get; set; }
        
        // Tiempos promedio en horas
        public double TiempoPromedioTotal { get; set; }
        public double TiempoPromedioEfectivo { get; set; }
        public double TiempoPromedioPausas { get; set; }
        
        // Por tipo
        public double TiempoPromedioMantenimiento { get; set; }
        public double TiempoPromedioReparacion { get; set; }
        
        // Estadísticas
        public double TiempoMinimo { get; set; }
        public double TiempoMaximo { get; set; }
        
        // Detalles
        public List<IngresoDetalladoDto> IngresosDetallados { get; set; } = new();
        
        // Pausas
        public int TotalPausas { get; set; }
        public double TiempoTotalPausas { get; set; }
        public List<PausaPorMotivoDto> PausasPorMotivo { get; set; } = new();
    }

    public class IngresoDetalladoDto
    {
        public IngresoTaller Ingreso { get; set; } = null!;
        public double TiempoTotal { get; set; }
        public int DiasTranscurridos { get; set; }
        public double TiempoPausas { get; set; }
        public double TiempoEfectivo { get; set; }
    }

    public class PausaPorMotivoDto
    {
        public string Motivo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double TiempoTotal { get; set; }
    }
}
