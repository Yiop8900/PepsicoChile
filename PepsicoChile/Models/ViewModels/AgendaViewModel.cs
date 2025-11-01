using PepsicoChile.Models;

namespace PepsicoChile.Models.ViewModels
{
    public class AgendaViewModel
    {
    public List<IngresoTaller> IngresosActivos { get; set; } = new();
  public List<IngresoTaller> IngresosCompletados { get; set; } = new();
    }
}
