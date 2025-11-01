using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Filters;
using PepsicoChile.Data;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Models.ViewModels;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("Administrador", "JefeTaller", "CoordinadorZona", "Supervisor")]
    public class ReportesController : Controller
    {
      private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
  {
       _context = context;
        }

     public IActionResult Index()
     {
 return View();
  }

        public IActionResult TiemposProceso()
{
      // Datos de ejemplo para reporte de tiempos
     ViewBag.TiempoPromedioMantenimiento = 4.5;
        ViewBag.TiempoPromedioReparacion = 8.2;
            ViewBag.TiempoPromedioTotal = 6.3;
     return View();
        }

        public IActionResult Productividad()
        {
            // Datos de ejemplo para reporte de productividad
            ViewBag.TareasCompletadas = 45;
      ViewBag.TareasEnProceso = 12;
    ViewBag.TareasPendientes = 8;
            ViewBag.EficienciaMecanicos = 87.5;
    return View();
        }

  public IActionResult Repuestos()
        {
       // Datos de ejemplo para reporte de repuestos
        ViewBag.RepuestosSolicitados = 32;
          ViewBag.RepuestosRecibidos = 28;
       ViewBag.RepuestosPendientes = 4;
     return View();
        }

    public async Task<IActionResult> VehiculosEstado()
    {
        // Obtener todos los vehículos
     var vehiculos = await _context.Vehiculos
          .OrderBy(v => v.Patente)
            .ToListAsync();

   // Estadísticas generales
   var estadisticas = new
      {
    Total = vehiculos.Count,
  Disponibles = vehiculos.Count(v => v.Estado == "Disponible"),
   EnRuta = vehiculos.Count(v => v.Estado == "En Ruta"),
       EnTaller = vehiculos.Count(v => v.Estado == "En Taller"),
      Programados = vehiculos.Count(v => v.Estado == "Programado"),
   PromedioKilometraje = vehiculos.Any() && vehiculos.Any(v => v.KilometrajeActual.HasValue)
   ? (int)vehiculos.Where(v => v.KilometrajeActual.HasValue).Average(v => v.KilometrajeActual.Value) 
    : 0
  };

        ViewBag.Estadisticas = estadisticas;
            
       return View(vehiculos);
 }

   public IActionResult HistoricoPausas()
        {
  // Datos de ejemplo para reporte de pausas
     ViewBag.TotalPausas = 15;
     ViewBag.TiempoTotalPausasHoras = 72;
  ViewBag.PausasPorEsperaRepuestos = 8;
            ViewBag.PausasPorFaltaPersonal = 7;
      return View();
        }
  }
}
