using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models
{
    public class DocumentoVehiculo
    {
     public int Id { get; set; }

        [Required]
        public int VehiculoId { get; set; }

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [StringLength(50)]
        public string TipoDocumento { get; set; } = string.Empty;

     [Required]
      [StringLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string RutaArchivo { get; set; } = string.Empty;

        public DateTime FechaSubida { get; set; } = DateTime.Now;

      [DataType(DataType.Date)]
   public DateTime? FechaVencimiento { get; set; }

        public int? UsuarioSubidaId { get; set; }

   public string? Descripcion { get; set; }

        public long TamañoBytes { get; set; }

        public bool Activo { get; set; } = true;

        // Navegación
public Vehiculo? Vehiculo { get; set; }
        public Usuario? UsuarioSubida { get; set; }
    }
}
