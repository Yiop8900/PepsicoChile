using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PepsicoChile.Models.ViewModels
{
    public class AgregarVehiculoViewModel
  {
        // Datos básicos
 [Required(ErrorMessage = "La patente es obligatoria")]
        [Display(Name = "Patente")]
   [StringLength(10)]
  public string Patente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La marca es obligatoria")]
        [Display(Name = "Marca")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        [Display(Name = "Modelo")]
        public string Modelo { get; set; } = string.Empty;

  [Required(ErrorMessage = "El año es obligatorio")]
        [Display(Name = "Año")]
      [Range(1900, 2100, ErrorMessage = "Año inválido")]
        public int Año { get; set; }

        [Required(ErrorMessage = "El tipo de vehículo es obligatorio")]
        [Display(Name = "Tipo de Vehículo")]
        public string TipoVehiculo { get; set; } = string.Empty;

        [Display(Name = "Número de Flota")]
        public string? NumeroFlota { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

[Display(Name = "Tipo de Combustible")]
        public string? Combustible { get; set; }

  [Display(Name = "Número de Motor")]
     public string? Motor { get; set; }

        [Display(Name = "Número de Chasis")]
     public string? Chasis { get; set; }

        [Display(Name = "Kilometraje Actual")]
    [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser positivo")]
        public int? KilometrajeActual { get; set; }

   [Display(Name = "Capacidad de Carga (ton)")]
        [Range(0, 100, ErrorMessage = "Capacidad inválida")]
  public decimal? CapacidadCarga { get; set; }

 [Display(Name = "Último Mantenimiento")]
        [DataType(DataType.Date)]
        public DateTime? UltimoMantenimiento { get; set; }

   [Display(Name = "Próximo Mantenimiento")]
     [DataType(DataType.Date)]
        public DateTime? ProximoMantenimiento { get; set; }

     [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
     public string? Observaciones { get; set; }

        // Documentos
     [Display(Name = "Revisión Técnica")]
    public IFormFile? RevisionTecnica { get; set; }

        [Display(Name = "Fecha Vencimiento Rev. Técnica")]
        [DataType(DataType.Date)]
    public DateTime? FechaVencimientoRevTecnica { get; set; }

        [Display(Name = "Seguro Obligatorio (SOAP)")]
        public IFormFile? SeguroObligatorio { get; set; }

        [Display(Name = "Fecha Vencimiento SOAP")]
  [DataType(DataType.Date)]
        public DateTime? FechaVencimientoSOAP { get; set; }

        [Display(Name = "Permiso de Circulación")]
      public IFormFile? PermisoCirculacion { get; set; }

        [Display(Name = "Fecha Vencimiento Permiso")]
        [DataType(DataType.Date)]
      public DateTime? FechaVencimientoPermiso { get; set; }

        [Display(Name = "Contrato de Arriendo/Compra")]
   public IFormFile? Contrato { get; set; }

        [Display(Name = "Manual del Vehículo")]
        public IFormFile? Manual { get; set; }

        [Display(Name = "Otros Documentos")]
   public List<IFormFile>? OtrosDocumentos { get; set; }
}
}
