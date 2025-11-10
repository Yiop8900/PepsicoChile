using PepsicoChile.Models;
using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models.ViewModels
{
    public class GestionRepuestosViewModel
    {
        public List<Repuesto> Repuestos { get; set; } = new();
        public List<Repuesto> RepuestosBajoStock { get; set; } = new();
        public int TotalRepuestos { get; set; }
        public int RepuestosBajoStockCount { get; set; }
        public decimal ValorTotalInventario { get; set; }
    }

    public class EntregarRepuestoViewModel
    {
        [Required]
        public int RepuestoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required]
        public int MecanicoId { get; set; }

        public int? VehiculoId { get; set; }
        public int? TareaTallerId { get; set; }

        [StringLength(500)]
        public string? Motivo { get; set; }

        public List<Usuario> MecanicosDisponibles { get; set; } = new();
        public List<Vehiculo> VehiculosDisponibles { get; set; } = new();
    }

    public class AgregarRepuestoViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        // El código se genera automáticamente, no es necesario ingresarlo
        [StringLength(50)]
        public string? CodigoRepuesto { get; set; }

        [StringLength(200)]
        public string? Categoria { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El stock inicial es obligatorio")]
        [Range(0, int.MaxValue)]
        public int StockInicial { get; set; }

        [Required(ErrorMessage = "El stock mínimo es obligatorio")]
        [Range(0, int.MaxValue)]
        public int StockMinimo { get; set; }

        [StringLength(50)]
        public string? Ubicacion { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? PrecioUnitario { get; set; }

        [Required(ErrorMessage = "El proveedor/marca es obligatorio")]
        [StringLength(100)]
        public string Proveedor { get; set; } = string.Empty;
    }

    public class HistorialMovimientosViewModel
    {
        public List<MovimientoRepuesto> Movimientos { get; set; } = new();
        public DateTime FechaInicio { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime FechaFin { get; set; } = DateTime.Now;
        public string? TipoMovimiento { get; set; }
        public int? RepuestoId { get; set; }
    }
}
