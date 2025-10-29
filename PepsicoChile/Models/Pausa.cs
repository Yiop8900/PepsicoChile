namespace PepsicoChile.Models
{
    public class Pausa
    {
        public int Id { get; set; }
        public int IngresoTallerId { get; set; }
        public IngresoTaller? IngresoTaller { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Motivo { get; set; } = string.Empty; // Espera de repuestos, Falta de personal, etc.
        public string Descripcion { get; set; } = string.Empty;
        public int? UsuarioRegistroId { get; set; }
        public Usuario? UsuarioRegistro { get; set; }
        public bool Activa { get; set; } = true;
    }
}
