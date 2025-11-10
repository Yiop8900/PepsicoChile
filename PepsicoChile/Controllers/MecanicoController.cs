using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PepsicoChile.Filters;
using PepsicoChile.Models;
using PepsicoChile.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

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
            var repuestos = await _context.SolicitudesRepuesto
                    .Where(r => r.TareaTallerId == id)
                    .ToListAsync();

            // Cargar lista de repuestos activos para el dropdown
            var repuestosDisponibles = await _context.Repuestos
                .Where(r => r.Activo)
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            ViewBag.Repuestos = repuestos;
            ViewBag.RepuestosDisponibles = repuestosDisponibles;

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
        public async Task<IActionResult> FinalizarTarea(int id, decimal tiempoRealHoras, string? observaciones, List<IFormFile>? imagenes)
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
            tarea.TiempoRealHoras = (int)Math.Round(tiempoRealHoras);

            if (!string.IsNullOrEmpty(observaciones))
            {
                tarea.Observaciones = string.IsNullOrEmpty(tarea.Observaciones)
                    ? observaciones
                    : tarea.Observaciones + "\n\n" + observaciones;
            }

            await _context.SaveChangesAsync();

            // Guardar imágenes si se adjuntaron
            if (imagenes != null && imagenes.Any())
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tareas");

                // Crear directorio si no existe
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

                foreach (var imagen in imagenes)
                {
                    if (imagen.Length > 0)
                    {
                        // Validar que sea una imagen
                        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

                        if (!extensionesPermitidas.Contains(extension))
                        {
                            continue; // Saltar archivos no válidos
                        }

                        // Generar nombre único
                        var nombreArchivo = $"tarea_{id}_{Guid.NewGuid()}{extension}";
                        var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);
                        var rutaRelativa = Path.Combine("uploads", "tareas", nombreArchivo).Replace("\\", "/");

                        // Guardar archivo
                        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            await imagen.CopyToAsync(stream);
                        }

                        // Crear registro en la base de datos
                        var imagenIngreso = new ImagenIngreso
                        {
                            IngresoTallerId = tarea.IngresoTallerId,
                            TipoImagen = "Trabajo Completado",
                            NombreArchivo = imagen.FileName,
                            RutaArchivo = rutaRelativa,
                            FechaSubida = DateTime.Now,
                            TamañoBytes = imagen.Length,
                            UsuarioSubidaId = usuarioId,
                            Descripcion = $"Imagen adjunta al finalizar tarea: {tarea.Descripcion}"
                        };

                        _context.ImagenesIngreso.Add(imagenIngreso);
                    }
                }

                await _context.SaveChangesAsync();
            }

            TempData["Mensaje"] = imagenes != null && imagenes.Any()
                ? $"Tarea finalizada exitosamente con {imagenes.Count} imagen(es) adjunta(s)"
                : "Tarea finalizada exitosamente";

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

            // Cargar lista de repuestos activos
            var repuestos = await _context.Repuestos
                .Where(r => r.Activo)
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            ViewBag.Tarea = tarea;
            ViewBag.Repuestos = repuestos;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarRepuesto(int tareaId, int? repuestoId, string? nombreManual, string? codigoManual, int cantidad, string? proveedor)
        {
            var mecanicoId = HttpContext.Session.GetInt32("UsuarioId");
            var mecanicoNombre = HttpContext.Session.GetString("UsuarioNombre");

            string nombre;
            string codigoRepuesto;
            string proveedorFinal;

            // Si seleccionó un repuesto del dropdown
            if (repuestoId.HasValue && repuestoId.Value > 0)
            {
                var repuesto = await _context.Repuestos.FindAsync(repuestoId.Value);
                if (repuesto == null)
                {
                    TempData["Error"] = "Repuesto no encontrado";
                    return RedirectToAction("SolicitarRepuesto", new { tareaId });
                }

                nombre = repuesto.Nombre;
                codigoRepuesto = repuesto.CodigoRepuesto;
                proveedorFinal = repuesto.Proveedor;
            }
            // Si ingresó manualmente
            else if (!string.IsNullOrEmpty(nombreManual))
            {
                nombre = nombreManual;
                codigoRepuesto = codigoManual ?? "";
                proveedorFinal = proveedor ?? "";
            }
            else
            {
                TempData["Error"] = "Debe seleccionar un repuesto o ingresar los datos manualmente";
                return RedirectToAction("SolicitarRepuesto", new { tareaId });
            }

            var solicitud = new SolicitudRepuesto
            {
                TareaTallerId = tareaId,
                Nombre = nombre,
                CodigoRepuesto = codigoRepuesto,
                Cantidad = cantidad,
                Proveedor = proveedorFinal,
                FechaSolicitud = DateTime.Now,
                Estado = "Solicitado",
                SolicitadoPorId = mecanicoId
            };

            _context.SolicitudesRepuesto.Add(solicitud);
            await _context.SaveChangesAsync();

            // Obtener información de la tarea para la notificación
            var tarea = await _context.TareasTaller
                .Include(t => t.IngresoTaller)
                .ThenInclude(i => i.Vehiculo)
                .FirstOrDefaultAsync(t => t.Id == tareaId);

            // Enviar notificación a todos los asistentes de repuestos
            var asistentesRepuestos = await _context.Usuarios
                .Where(u => u.Rol == "AsistenteRepuestos" && u.Activo)
                .ToListAsync();

            foreach (var asistente in asistentesRepuestos)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = asistente.Id,
                    Titulo = "Nueva solicitud de repuesto",
                    Mensaje = $"{mecanicoNombre} solicita {cantidad} unidad(es) de {nombre} (Código: {codigoRepuesto})" +
                      (tarea?.IngresoTaller?.Vehiculo != null ? $" para {tarea.IngresoTaller.Vehiculo.Patente}" : ""),
                    Tipo = "Tarea",
                    TareaId = tareaId,
                    UrlAccion = $"/SolicitudesRepuesto/Index",
                    FechaCreacion = DateTime.Now,
                    Leida = false
                };

                _context.Notificaciones.Add(notificacion);
            }

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
