using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Filters;
using PepsicoChile.Data;
using Microsoft.EntityFrameworkCore;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    public class HomeController : Controller
  {
   private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
      {
   _context = context;
 }

        public async Task<IActionResult> Index()
   {
 var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
    var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
   var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
      
// Obtener datos reales del dashboard
     var ingresosRecientes = await _context.IngresosTaller
      .Include(i => i.Vehiculo)
    .Include(i => i.Chofer)
   .Include(i => i.MecanicoAsignado)
  .Where(i => i.Estado != "Completado" && i.Estado != "Cancelado")
        .OrderByDescending(i => i.FechaProgramada)
.Take(5)
 .ToListAsync();

      List<TareaTaller> tareasUrgentes;
  if (usuarioRol == "Mecanico")
        {
    // Si es mecánico, solo sus tareas
   tareasUrgentes = await _context.TareasTaller
   .Include(t => t.IngresoTaller)
       .ThenInclude(i => i.Vehiculo)
   .Where(t => t.MecanicoAsignadoId == usuarioId 
          && t.Estado != "Completada" 
      && t.Prioridad == "Alta")
   .OrderBy(t => t.FechaAsignacion)
     .Take(5)
.ToListAsync();
       }
   else
  {
   // Para otros roles, todas las tareas urgentes
   tareasUrgentes = await _context.TareasTaller
        .Include(t => t.IngresoTaller)
 .ThenInclude(i => i.Vehiculo)
    .Include(t => t.MecanicoAsignado)
     .Where(t => t.Estado != "Completada" && t.Prioridad == "Alta")
    .OrderBy(t => t.FechaAsignacion)
  .Take(5)
       .ToListAsync();
  }

       var model = new DashboardViewModel
     {
NombreUsuario = usuarioNombre ?? "Usuario",
     RolUsuario = usuarioRol ?? "Sin Rol",
        VehiculosEnTaller = await _context.Vehiculos.CountAsync(v => v.Estado == "En Taller"),
VehiculosProgramados = await _context.IngresosTaller.CountAsync(i => i.Estado == "Programado"),
    TareasPendientes = usuarioRol == "Mecanico"
      ? await _context.TareasTaller.CountAsync(t => t.MecanicoAsignadoId == usuarioId && t.Estado == "Pendiente")
     : await _context.TareasTaller.CountAsync(t => t.Estado == "Pendiente"),
    TareasEnProceso = usuarioRol == "Mecanico"
     ? await _context.TareasTaller.CountAsync(t => t.MecanicoAsignadoId == usuarioId && t.Estado == "En Proceso")
   : await _context.TareasTaller.CountAsync(t => t.Estado == "En Proceso"),
      IngresosRecientes = ingresosRecientes,
  TareasUrgentes = tareasUrgentes
     };

     return View(model);
   }

        public IActionResult CambiarRol(string rol)
   {
   // Simulación de cambio de rol para testing
   HttpContext.Session.SetString("RolActual", rol);
         HttpContext.Session.SetString("UsuarioRol", rol);
      return RedirectToAction("Index");
 }

  public IActionResult Error()
{
  return View();
        }
    }
}
