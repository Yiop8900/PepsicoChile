using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models
{
    public class Repuesto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(50)]
        public string CodigoRepuesto { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Categoria { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public int StockActual { get; set; } = 0;

        [Required]
        public int StockMinimo { get; set; } = 0;

        [StringLength(50)]
        public string? Ubicacion { get; set; }

        public decimal? PrecioUnitario { get; set; }

        [Required(ErrorMessage = "El proveedor/marca es obligatorio")]
        [StringLength(100)]
        public string Proveedor { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }

    public class MovimientoRepuesto
    {
        public int Id { get; set; }

        [Required]
        public int RepuestoId { get; set; }
        public Repuesto? Repuesto { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoMovimiento { get; set; } = string.Empty; // Entrada, Salida, Ajuste

        [Required]
        public int Cantidad { get; set; }

        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }

        [StringLength(500)]
        public string? Motivo { get; set; }

        public int? VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        public int? TareaTallerId { get; set; }
        public TareaTaller? TareaTaller { get; set; }

        public int? MecanicoId { get; set; }
        public Usuario? Mecanico { get; set; }

        public int? UsuarioRegistroId { get; set; }
        public Usuario? UsuarioRegistro { get; set; }

        public DateTime FechaMovimiento { get; set; } = DateTime.Now;
    }
}
