using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Filters;
using PepsicoChile.Models;
using PepsicoChile.Data;
using Microsoft.EntityFrameworkCore;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("Administrador", "Mecanico")]
    public class MecanicoController : Controller
    {
 private readonly ApplicationDbContext _context;

        public MecanicoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("MisTareas");
        }

public async Task<IActionResult> MisTareas()
        {
  var mecanicoId = HttpContext.Session.GetInt32("UsuarioId");
      
      var tareas = await _context.TareasTaller
         .Include(t => t.IngresoTaller)
         .ThenInclude(i => i.Vehiculo)
       .Include(t => t.IngresoTaller)
         .ThenInclude(i => i.Chofer)
     .Include(t => t.MecanicoAsignado)
              .Where(t => t.MecanicoAsignadoId == mecanicoId && t.Estado != "Completada")
     .OrderByDescending(t => t.Prioridad == "Alta")
  .ThenByDescending(t => t.Prioridad == "Media")
     .ThenBy(t => t.FechaAsignacion)
  .ToListAsync();

     return View(tareas);
        }

    public async Task<IActionResult> DetalleTarea(int id)
        {
            var tarea = await _context.TareasTaller
      .Include(t => t.IngresoTaller)
            .ThenInclude(i => i.Vehiculo)
.Include(t => t.IngresoTaller)
         .ThenInclude(i => i.Chofer)
                .Include(t => t.IngresoTaller)
      .ThenInclude(i => i.Supervisor)
     .Include(t => t.MecanicoAsignado)
                .FirstOrDefaultAsync(t => t.Id == id);

      if (tarea == null)
            {
    return NotFound();
            }

       // Verificar que la tarea pertenece al mecánico actual
          var mecanicoId = HttpContext.Session.GetInt32("UsuarioId");
   if (tarea.MecanicoAsignadoId != mecanicoId && HttpContext.Session.GetString("UsuarioRol") != "Administrador")
            {
       TempData["Error"] = "No tienes permiso para ver esta tarea";
   return RedirectToAction("MisTareas");
 }

 // Obtener repuestos solicitados para esta tarea
    var repuestos = await _context.Repuestos
        .Where(r => r.TareaTallerId == id)
   .ToListAsync();

       ViewBag.Repuestos = repuestos;

 return View(tarea);
     }

    [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarTarea(int id)
        {
    var tarea = await _context.TareasTaller.FindAsync(id);
      
 if (tarea == null)
            {
    return NotFound();
     }

    if (tarea.Estado == "Pendiente")
   {
tarea.Estado = "En Proceso";
   tarea.FechaInicio = DateTime.Now;
         
     await _context.SaveChangesAsync();
    
  TempData["Mensaje"] = "Tarea iniciada correctamente";
       }
            else
  {
             TempData["Error"] = "La tarea ya fue iniciada";
            }

       return RedirectToAction("DetalleTarea", new { id });
     }

        [HttpGet]
     public async Task<IActionResult> FinalizarTarea(int id)
 {
        var tarea = await _context.TareasTaller
      .Include(t => t.IngresoTaller)
              .ThenInclude(i => i.Vehiculo)
     .FirstOrDefaultAsync(t => t.Id == id);

   if (tarea == null)
            {
                return NotFound();
            }

    return View(tarea);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
      public async Task<IActionResult> FinalizarTarea(int id, int tiempoRealHoras, string? observaciones)
      {
  var tarea = await _context.TareasTaller
          .Include(t => t.IngresoTaller)
        .FirstOrDefaultAsync(t => t.Id == id);

         if (tarea == null)
            {
        return NotFound();
            }

        tarea.Estado = "Completada";
      tarea.FechaFinalizacion = DateTime.Now;
   tarea.TiempoRealHoras = tiempoRealHoras;
         
        if (!string.IsNullOrEmpty(observaciones))
            {
         tarea.Observaciones = string.IsNullOrEmpty(tarea.Observaciones) 
        ? observaciones 
          : tarea.Observaciones + "\n\n" + observaciones;
 }

        await _context.SaveChangesAsync();

     TempData["Mensaje"] = "Tarea finalizada exitosamente";
   return RedirectToAction("MisTareas");
   }

      [HttpGet]
        public async Task<IActionResult> SolicitarRepuesto(int tareaId)
    {
 var tarea = await _context.TareasTaller
         .Include(t => t.IngresoTaller)
        .ThenInclude(i => i.Vehiculo)
      .FirstOrDefaultAsync(t => t.Id == tareaId);

            if (tarea == null)
            {
return NotFound();
       }

            ViewBag.Tarea = tarea;
        return View();
        }

     [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarRepuesto(int tareaId, string nombre, string codigoRepuesto, int cantidad, string? proveedor)
        {
    var repuesto = new Repuesto
      {
     TareaTallerId = tareaId,
             Nombre = nombre,
     CodigoRepuesto = codigoRepuesto,
    Cantidad = cantidad,
         Proveedor = proveedor,
        FechaSolicitud = DateTime.Now,
          Estado = "Solicitado"
        };

            _context.Repuestos.Add(repuesto);
  await _context.SaveChangesAsync();

    TempData["Mensaje"] = "Repuesto solicitado exitosamente";
            return RedirectToAction("DetalleTarea", new { id = tareaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
   public async Task<IActionResult> RegistrarObservacion(int tareaId, string observacion)
   {
            var tarea = await _context.TareasTaller.FindAsync(tareaId);
          
            if (tarea == null)
        {
         return NotFound();
          }

 if (string.IsNullOrEmpty(tarea.Observaciones))
       {
              tarea.Observaciones = $"[{DateTime.Now:dd/MM/yyyy HH:mm}] {observacion}";
  }
         else
   {
 tarea.Observaciones += $"\n\n[{DateTime.Now:dd/MM/yyyy HH:mm}] {observacion}";
  }

          await _context.SaveChangesAsync();

    TempData["Mensaje"] = "Observación registrada exitosamente";
            return RedirectToAction("DetalleTarea", new { id = tareaId });
        }

        [HttpGet]
        public async Task<IActionResult> HistorialTareas()
        {
            var mecanicoId = HttpContext.Session.GetInt32("UsuarioId");
            
        var tareas = await _context.TareasTaller
          .Include(t => t.IngresoTaller)
  .ThenInclude(i => i.Vehiculo)
    .Where(t => t.MecanicoAsignadoId == mecanicoId && t.Estado == "Completada")
     .OrderByDescending(t => t.FechaFinalizacion)
           .Take(50)
       .ToListAsync();

    return View(tareas);
        }
    }
}
