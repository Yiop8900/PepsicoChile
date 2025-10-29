using Microsoft.AspNetCore.Mvc;

namespace PepsicoChile.Controllers
{
    public class ReportesController : Controller
 {
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

        public IActionResult VehiculosEstado()
        {
  // Datos de ejemplo para reporte de estado de vehículos
   ViewBag.VehiculosEnTaller = 8;
      ViewBag.VehiculosDisponibles = 45;
     ViewBag.VehiculosEnRuta = 32;
    return View();
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
