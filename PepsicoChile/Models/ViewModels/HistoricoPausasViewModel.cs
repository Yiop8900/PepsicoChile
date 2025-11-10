using PepsicoChile.Models;

namespace PepsicoChile.Models.ViewModels
{
    public class HistoricoPausasViewModel
    {
        // Totales
        public int TotalPausas { get; set; }
        public int PausasActivas { get; set; }
        public int PausasFinalizadas { get; set; }
        public double TiempoTotalHoras { get; set; }
        public double TiempoPromedioHoras { get; set; }

        // Análisis por motivo
        public List<PausaPorMotivoDetalladaDto> PausasPorMotivo { get; set; } = new();

        // Análisis por vehículo
        public List<PausaPorVehiculoDto> PausasPorVehiculo { get; set; } = new();

        // Tendencia temporal
        public List<PausaPorMesDto> PausasPorMes { get; set; } = new();

        // Pausas recientes
        public List<Pausa> PausasRecientes { get; set; } = new();

        // Estadísticas adicionales
        public string MotivoPrincipal { get; set; } = string.Empty;
        public double TiempoPausaMasLarga { get; set; }
        public double TiempoPausaMasCorta { get; set; }
        public Dictionary<string, double> DuracionPorTipoVehiculo { get; set; } = new();
    }

    public class PausaPorMotivoDetalladaDto
    {
        public string Motivo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double TiempoTotal { get; set; }
        public double TiempoPromedio { get; set; }
        public double TiempoMinimo { get; set; }
        public double TiempoMaximo { get; set; }
    }

    public class PausaPorVehiculoDto
    {
        public int VehiculoId { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string MarcaModelo { get; set; } = string.Empty;
        public int CantidadPausas { get; set; }
        public double TiempoTotal { get; set; }
    }

    public class PausaPorMesDto
    {
        public int Año { get; set; }
        public int Mes { get; set; }
        public int Cantidad { get; set; }
        public double TiempoTotal { get; set; }
        
        public string NombreMes => new DateTime(Año, Mes, 1).ToString("MMMM yyyy");
    }
}
