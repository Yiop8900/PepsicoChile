using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;
using PepsicoChile.Filters;
using PepsicoChile.Models;
using PepsicoChile.Services;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("Administrador", "JefeTaller", "Supervisor", "AsistenteRepuestos", "Mecanico")]
    public class SolicitudesRepuestoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificacionService _notificacionService;

        public SolicitudesRepuestoController(ApplicationDbContext context, INotificacionService notificacionService)
        {
            _context = context;
            _notificacionService = notificacionService;
        }

        // GET: Listado de Solicitudes
        public async Task<IActionResult> Index()
        {
            var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            var solicitudesQuery = _context.SolicitudesRepuesto
                .Include(s => s.TareaTaller)
                    .ThenInclude(t => t!.IngresoTaller)
                        .ThenInclude(i => i!.Vehiculo)
                .Include(s => s.SolicitadoPor)
                .AsQueryable();

            // Si es mecánico, solo ver sus solicitudes
            if (usuarioRol == "Mecanico")
            {
                solicitudesQuery = solicitudesQuery.Where(s => s.SolicitadoPorId == usuarioId);
            }

            var solicitudes = await solicitudesQuery
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            // Estadísticas
            ViewBag.TotalSolicitudes = solicitudes.Count;
            ViewBag.Solicitadas = solicitudes.Count(s => s.Estado == "Solicitado");
            ViewBag.EnTransito = solicitudes.Count(s => s.Estado == "En Tránsito");
            ViewBag.Entregadas = solicitudes.Count(s => s.Estado == "Entregado");
            ViewBag.Instaladas = solicitudes.Count(s => s.Estado == "Instalado");

            return View(solicitudes);
        }

        // GET: Mis Solicitudes (para mecánicos)
        public async Task<IActionResult> MisSolicitudes()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            var solicitudes = await _context.SolicitudesRepuesto
                .Include(s => s.TareaTaller)
                    .ThenInclude(t => t!.IngresoTaller)
                        .ThenInclude(i => i!.Vehiculo)
                .Where(s => s.SolicitadoPorId == usuarioId)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        // GET: Crear Solicitud
        [HttpGet]
        public async Task<IActionResult> Crear(int? tareaTallerId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            // Obtener tareas asignadas al usuario
            var tareasDisponibles = await _context.TareasTaller
                .Include(t => t.IngresoTaller)
                    .ThenInclude(i => i!.Vehiculo)
                .Where(t => t.MecanicoAsignadoId == usuarioId && 
                           (t.Estado == "En Proceso" || t.Estado == "Pendiente"))
                .ToListAsync();

            ViewBag.TareasDisponibles = tareasDisponibles;
            ViewBag.TareaSeleccionada = tareaTallerId;

            return View();
        }

        // POST: Crear Solicitud
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(SolicitudRepuesto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                    var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");

                    model.FechaSolicitud = DateTime.Now;
                    model.SolicitadoPorId = usuarioId;
                    model.Estado = "Solicitado";

                    _context.SolicitudesRepuesto.Add(model);
                    await _context.SaveChangesAsync();

                    // Obtener información de la tarea para el mensaje
                    var tarea = await _context.TareasTaller
                        .Include(t => t.IngresoTaller)
                            .ThenInclude(i => i!.Vehiculo)
                        .FirstOrDefaultAsync(t => t.Id == model.TareaTallerId);

                    var vehiculoInfo = tarea?.IngresoTaller?.Vehiculo?.Patente ?? "N/A";

                    // Enviar notificación a todos los Asistentes de Repuestos
                    var asistentesRepuestos = await _context.Usuarios
                        .Where(u => u.Rol == "AsistenteRepuestos" && u.Activo)
                        .ToListAsync();

                    foreach (var asistente in asistentesRepuestos)
                    {
                        await _notificacionService.CrearNotificacion(
                            usuarioId: asistente.Id,
                            titulo: "Nueva Solicitud de Repuesto",
                            mensaje: $"{usuarioNombre} solicitó {model.Cantidad}x {model.Nombre} para el vehículo {vehiculoInfo}",
                            tipo: "Repuesto",
                            urlAccion: $"/SolicitudesRepuesto/Detalle/{model.Id}"
                        );

                        Console.WriteLine($"[NOTIF] Notificación creada para {asistente.Nombre} {asistente.Apellido} (ID: {asistente.Id})");
                    }

                    Console.WriteLine($"[NOTIF] Total de notificaciones enviadas: {asistentesRepuestos.Count}");

                    TempData["Mensaje"] = $"Solicitud de '{model.Nombre}' creada exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al crear solicitud: {ex.Message}");
                }
            }

            // Recargar tareas si hay error
            var usuarioId2 = HttpContext.Session.GetInt32("UsuarioId");
            ViewBag.TareasDisponibles = await _context.TareasTaller
                .Include(t => t.IngresoTaller)
                    .ThenInclude(i => i!.Vehiculo)
                .Where(t => t.MecanicoAsignadoId == usuarioId2 && 
                           (t.Estado == "En Proceso" || t.Estado == "Pendiente"))
                .ToListAsync();

            return View(model);
        }

        // POST: Cambiar Estado
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "JefeTaller", "Supervisor", "AsistenteRepuestos", "Mecanico")]
        public async Task<IActionResult> CambiarEstado([FromBody] CambiarEstadoRequest request)
        {
            try
            {
                var usuarioRol = HttpContext.Session.GetString("UsuarioRol");
                var solicitud = await _context.SolicitudesRepuesto.FindAsync(request.Id);
                
                if (solicitud == null)
                {
                    return Json(new { success = false, message = "Solicitud no encontrada" });
                }

                // Validar permisos según rol
                if (usuarioRol == "AsistenteRepuestos")
                {
                    // AsistenteRepuestos solo puede cambiar a Solicitado, En Tránsito y Entregado
                    if (request.NuevoEstado == "Instalado")
                    {
                        return Json(new { success = false, message = "Solo el mecánico puede marcar como Instalado" });
                    }
                }
                else if (usuarioRol == "Mecanico")
                {
                    // Mecánico solo puede cambiar a Instalado (cuando ya está Entregado)
                    if (request.NuevoEstado != "Instalado")
                    {
                        return Json(new { success = false, message = "Los mecánicos solo pueden marcar repuestos como Instalado" });
                    }
                    
                    if (solicitud.Estado != "Entregado")
                    {
                        return Json(new { success = false, message = "Solo se pueden instalar repuestos que ya han sido entregados" });
                    }
                }

                solicitud.Estado = request.NuevoEstado;

                if (request.NuevoEstado == "Entregado")
                {
                    solicitud.FechaEntrega = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Estado actualizado a '{request.NuevoEstado}'" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Clase auxiliar para el request
        public class CambiarEstadoRequest
        {
            public int Id { get; set; }
            public string NuevoEstado { get; set; } = string.Empty;
        }

        // GET: Test Notificaciones (solo para depuración)
        [HttpGet]
        [AuthorizeRole("Administrador")]
        public async Task<IActionResult> TestNotificaciones()
        {
            try
            {
                var asistentesRepuestos = await _context.Usuarios
                    .Where(u => u.Rol == "AsistenteRepuestos" && u.Activo)
                    .ToListAsync();

                var notificacionesRecientes = await _context.Notificaciones
                    .Include(n => n.Usuario)
                    .Where(n => n.Tipo == "Repuesto")
                    .OrderByDescending(n => n.FechaCreacion)
                    .Take(10)
                    .ToListAsync();

                return Json(new
                {
                    asistentesCount = asistentesRepuestos.Count,
                    asistentes = asistentesRepuestos.Select(a => new
                    {
                        a.Id,
                        a.Nombre,
                        a.Apellido,
                        a.Email,
                        a.Activo
                    }),
                    notificacionesRecientes = notificacionesRecientes.Select(n => new
                    {
                        n.Id,
                        n.Titulo,
                        n.Mensaje,
                        n.FechaCreacion,
                        Usuario = $"{n.Usuario?.Nombre} {n.Usuario?.Apellido}",
                        n.Leida
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // GET: Detalle de Solicitud
        public async Task<IActionResult> Detalle(int id)
        {
            var solicitud = await _context.SolicitudesRepuesto
                .Include(s => s.TareaTaller)
                    .ThenInclude(t => t!.IngresoTaller)
                        .ThenInclude(i => i!.Vehiculo)
                .Include(s => s.TareaTaller)
                    .ThenInclude(t => t!.MecanicoAsignado)
                .Include(s => s.SolicitadoPor)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            return View(solicitud);
        }

        // POST: Eliminar Solicitud
        [HttpPost]
        [AuthorizeRole("Administrador", "JefeTaller", "Supervisor")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var solicitud = await _context.SolicitudesRepuesto.FindAsync(id);
                if (solicitud == null)
                {
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction("Index");
                }

                _context.SolicitudesRepuesto.Remove(solicitud);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Solicitud eliminada exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar solicitud: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
