using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;
using PepsicoChile.Filters;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;
using PepsicoChile.Services;
using System.Threading;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("Administrador", "JefeTaller", "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificacionService _notificacionService;

        public SupervisorController(ApplicationDbContext context, INotificacionService notificacionService)
        {
            _context = context;
            _notificacionService = notificacionService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Agenda");
        }

        public async Task<IActionResult> Agenda()
        {
            // Obtener ingresos activos (programados, en proceso, pausados)
            var ingresosActivos = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Include(i => i.MecanicoAsignado)
                .Where(i => i.Estado == "Programado" || i.Estado == "En Proceso" || i.Estado == "Pausado")
                .OrderBy(i => i.FechaProgramada)
                .ToListAsync();

            // Obtener ingresos completados recientes (últimos 30 días)
            // Ahora solo se consideran "completados" cuando tienen FechaSalidaReal (salieron de la instalación)
            var fechaLimite = DateTime.Now.AddDays(-30);
            var ingresosCompletados = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Include(i => i.MecanicoAsignado)
                .Where(i => i.FechaSalidaReal.HasValue 
                    && i.FechaSalidaReal.Value >= fechaLimite)
                .OrderByDescending(i => i.FechaSalidaReal)
                .Take(20)
                .ToListAsync();

            var model = new AgendaViewModel
            {
                IngresosActivos = ingresosActivos,
                IngresosCompletados = ingresosCompletados
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ProgramarIngreso()
        {
            var model = new ProgramarIngresoViewModel
            {
                VehiculosDisponibles = await _context.Vehiculos
                    .Where(v => v.Estado == "Disponible" || v.Estado == "En Ruta")
                    .OrderBy(v => v.Patente)
                    .ToListAsync(),
                ChofersDisponibles = await _context.Usuarios
                    .Where(u => u.Rol == "Chofer" && u.Activo)
                    .OrderBy(u => u.Nombre)
                    .ToListAsync(),

                MecanicosDisponibles = await _context.Usuarios
                    .Where(u => u.Rol == "Mecanico" && u.Activo)
                    .OrderBy(u => u.Nombre)
                    .ToListAsync(),

                FechaProgramada = DateTime.Now.AddHours(3),
                FechaSalidaEstimada = DateTime.Now.AddDays(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProgramarIngreso(ProgramarIngresoViewModel model)
        {
            // Validar que la fecha programada sea al menos 3 horas después de ahora
            if (model.FechaProgramada < DateTime.Now.AddHours(3))
            {
                ModelState.AddModelError("FechaProgramada", "La fecha programada debe ser al menos 3 horas después de la hora actual");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var supervisorId = HttpContext.Session.GetInt32("UsuarioId");

                    var ingreso = new IngresoTaller
                    {
                        VehiculoId = model.VehiculoId,
                        ChoferId = model.ChoferId,
                        MecanicoAsignadoId = model.MecanicoAsignadoId,
                        FechaProgramada = model.FechaProgramada,
                        MotivoIngreso = model.MotivoIngreso,
                        DescripcionProblema = model.DescripcionProblema,
                        RequiereRepuestos = model.RequiereRepuestos,
                        KilometrajeIngreso = model.KilometrajeIngreso,
                        FechaSalidaEstimada = model.FechaSalidaEstimada,
                        ObservacionesChofer = model.ObservacionesChofer,
                        Estado = "Programado",
                        SupervisorId = supervisorId
                    };

                    _context.IngresosTaller.Add(ingreso);

                    // Cambiar estado del vehículo
                    var vehiculo = await _context.Vehiculos.FindAsync(model.VehiculoId);
                    if (vehiculo != null)
                    {
                        vehiculo.Estado = "Programado";
                        vehiculo.FechaActualizacion = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    // Obtener información completa para las notificaciones
                    ingreso = await _context.IngresosTaller
                        .Include(i => i.Vehiculo)
                        .Include(i => i.Chofer)
                        .FirstOrDefaultAsync(i => i.Id == ingreso.Id);

                    if (ingreso != null)
                    {
                        // 1. NOTIFICAR AL GUARDIA DE ACCESO (importante para control de ingreso)
                        var guardias = await _context.Usuarios
                            .Where(u => u.Rol == "GuardiaAcceso" && u.Activo)
                            .ToListAsync();

                        foreach (var guardia in guardias)
                        {
                            var mensajeGuardia = $"*Nuevo Ingreso Programado*\n\n" +
                                $"Vehículo: *{ingreso.Vehiculo?.Patente}* - {ingreso.Vehiculo?.Marca} {ingreso.Vehiculo?.Modelo}\n" +
                                $"Chofer: {ingreso.Chofer?.Nombre} {ingreso.Chofer?.Apellido}\n" +
                                $"Fecha programada: {model.FechaProgramada:dd/MM/yyyy HH:mm}\n" +
                                $"Motivo: {model.MotivoIngreso}\n\n" +
                                $"Esté preparado para registrar la llegada del vehículo.";

                            await _notificacionService.CrearNotificacion(
                                usuarioId: guardia.Id,
                                titulo: "Nuevo Ingreso Programado",
                                mensaje: mensajeGuardia,
                                tipo: "Info",
                                ingresoId: ingreso.Id,
                                vehiculoId: ingreso.VehiculoId,
                                urlAccion: $"/GuardiaAcceso/Agenda"
                            );

                            Console.WriteLine($"[NOTIFICACION] Notificación enviada al guardia: {guardia.Nombre} {guardia.Apellido}");
                        }

                        // 2. NOTIFICAR AL CHOFER
                        var mensajeChofer = $"*Ingreso al Taller Programado*\n\n" +
                            $"Su vehículo *{ingreso.Vehiculo?.Patente}* ha sido programado para ingreso al taller.\n\n" +
                            $"Fecha: {model.FechaProgramada:dd/MM/yyyy HH:mm}\n" +
                            $"Motivo: {model.MotivoIngreso}\n" +
                            $"Lugar: Taller PepsiCo Chile\n\n" +
                            $"Por favor, llegue puntualmente a la hora programada.";

                        await _notificacionService.CrearNotificacion(
                            usuarioId: ingreso.ChoferId,
                            titulo: "Ingreso al Taller Programado",
                            mensaje: mensajeChofer,
                            tipo: "Info",
                            ingresoId: ingreso.Id,
                            vehiculoId: ingreso.VehiculoId,
                            urlAccion: $"/Chofer/DetalleIngreso/{ingreso.Id}"
                        );

                        // 3. NOTIFICAR AL MECÁNICO ASIGNADO (si hay uno)
                        if (model.MecanicoAsignadoId.HasValue)
                        {
                            var mecanico = await _context.Usuarios.FindAsync(model.MecanicoAsignadoId.Value);
                            if (mecanico != null)
                            {
                                var mensajeMecanico = $"*Nuevo Trabajo Asignado*\n\n" +
                                    $"Se te ha asignado un nuevo trabajo:\n" +
                                    $"Vehículo: *{ingreso.Vehiculo?.Patente}*\n" +
                                    $"Fecha programada: {model.FechaProgramada:dd/MM/yyyy HH:mm}\n" +
                                    $"Motivo: {model.MotivoIngreso}\n" +
                                    $"Requiere repuestos: {(model.RequiereRepuestos ? "Sí" : "No")}\n\n" +
                                    $"Prepárate para recibir el vehículo.";

                                await _notificacionService.CrearNotificacion(
                                    usuarioId: mecanico.Id,
                                    titulo: "Nuevo Trabajo Asignado",
                                    mensaje: mensajeMecanico,
                                    tipo: "Tarea",
                                    ingresoId: ingreso.Id,
                                    vehiculoId: ingreso.VehiculoId,
                                    urlAccion: $"/Mecanico/MisTareas"
                                );

                                Console.WriteLine($"[NOTIFICACION] Notificación enviada al mecánico: {mecanico.Nombre} {mecanico.Apellido}");
                            }
                        }
                    }

                    TempData["Mensaje"] = $"Ingreso programado exitosamente para el {model.FechaProgramada:dd/MM/yyyy HH:mm}. Se han notificado a todos los involucrados.";
                    return RedirectToAction("Agenda");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al programar el ingreso: {ex.Message}");
                }
            }

            model.VehiculosDisponibles = await _context.Vehiculos
                .Where(v => v.Estado == "Disponible" || v.Estado == "En Ruta")
                .OrderBy(v => v.Patente)
                .ToListAsync();

            model.ChofersDisponibles = await _context.Usuarios
                .Where(u => u.Rol == "Chofer" && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            model.MecanicosDisponibles = await _context.Usuarios
                .Where(u => u.Rol == "Mecanico" && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> MonitoreoGeneral()
        {
            // Obtener ingresos activos (incluye "Completado" porque aún no han salido de la instalación)
            var ingresosActivos = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Include(i => i.MecanicoAsignado)
                .Where(i => !i.FechaSalidaReal.HasValue && i.Estado != "Cancelado")
                .OrderByDescending(i => i.FechaProgramada)
                .ToListAsync();

            // Obtener ingresos que ya salieron (últimos 15 días)
            var fechaLimite = DateTime.Now.AddDays(-15);
            var ingresosCompletados = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Include(i => i.MecanicoAsignado)
                .Where(i => i.FechaSalidaReal.HasValue 
                    && i.FechaSalidaReal.Value >= fechaLimite)
                .OrderByDescending(i => i.FechaSalidaReal)
                .Take(15)
                .ToListAsync();

            var model = new MonitoreoGeneralViewModel
            {
                IngresosActivos = ingresosActivos,
                IngresosCompletados = ingresosCompletados
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GestionarIngreso(int id)
        {
            var ingreso = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Include(i => i.MecanicoAsignado)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
            {
                return NotFound();
            }

            var tareas = await _context.TareasTaller
                .Include(t => t.MecanicoAsignado)
                .Where(t => t.IngresoTallerId == id)
                .OrderByDescending(t => t.FechaAsignacion)
                .ToListAsync();

            var pausas = await _context.Pausas
                .Where(p => p.IngresoTallerId == id)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();

            var documentos = await _context.Documentos
                .Include(d => d.UsuarioSubida)
                .Where(d => d.IngresoTallerId == id)
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();

            // Obtener imágenes del ingreso subidas por el guardia
            var imagenesIngreso = await _context.ImagenesIngreso
                .Include(i => i.UsuarioSubida)
                .Where(i => i.IngresoTallerId == id)
                .OrderByDescending(i => i.FechaSubida)
                .ToListAsync();

            var mecanicos = await _context.Usuarios
                .Where(u => u.Rol == "Mecanico" && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            var model = new GestionarIngresoViewModel
            {
                Ingreso = ingreso,
                Tareas = tareas,
                Pausas = pausas,
                Documentos = documentos,
                MecanicosDisponibles = mecanicos,
                PuedeEditarTareas = ingreso.Estado != "Completado" && ingreso.Estado != "Cancelado"
            };

            ViewBag.ImagenesIngreso = imagenesIngreso;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarIngreso(int id)
        {
            var ingreso = await _context.IngresosTaller
              .Include(i => i.Vehiculo)
              .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
            {
                return NotFound();
            }

            ingreso.FechaIngresoReal = DateTime.Now;
            ingreso.Estado = "En Proceso";

            if (ingreso.Vehiculo != null)
            {
                ingreso.Vehiculo.Estado = "En Taller";
                ingreso.Vehiculo.FechaActualizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Ingreso iniciado correctamente";
            return RedirectToAction("GestionarIngreso", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarTarea(int ingresoId, GestionarIngresoViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NuevaTareaDescripcion) || !model.NuevaTareaMecanicoId.HasValue)
            {
                TempData["Error"] = "Debe especificar la descripción y el mecánico";
                return RedirectToAction("GestionarIngreso", new { id = ingresoId });
            }

            var tarea = new TareaTaller
            {
                IngresoTallerId = ingresoId,
                Descripcion = model.NuevaTareaDescripcion,
                MecanicoAsignadoId = model.NuevaTareaMecanicoId.Value,
                Prioridad = model.NuevaTareaPrioridad ?? "Media",
                Estado = "Pendiente",
                FechaAsignacion = DateTime.Now
            };

            _context.TareasTaller.Add(tarea);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[TAREA] Nueva tarea creada - ID: {tarea.Id}, Prioridad: {tarea.Prioridad}");

            // Obtener información para la notificación
            var ingreso = await _context.IngresosTaller
              .Include(i => i.Vehiculo)
                        .FirstOrDefaultAsync(i => i.Id == ingresoId);

            var mecanico = await _context.Usuarios.FindAsync(model.NuevaTareaMecanicoId.Value);

            // Crear notificación para el mecánico
            if (mecanico != null && ingreso != null)
            {
                Console.WriteLine($"[NOTIFICACION] Creando notificación para mecánico: {mecanico.Nombre} {mecanico.Apellido}");
                Console.WriteLine($"[NOTIFICACION] Teléfono del mecánico: {mecanico.Telefono}");
                Console.WriteLine($"[NOTIFICACION] Tipo de notificación: {(tarea.Prioridad == "Alta" ? "Urgente" : "Tarea")}");

                var tipoNotificacion = tarea.Prioridad == "Alta" ? "Urgente" : "Tarea";
                var mensajeWhatsApp = $"*Nueva Tarea Asignada*\n\n" +
                    $"Se te ha asignado una tarea de prioridad *{tarea.Prioridad}*:\n" +
                    $"{tarea.Descripcion}\n\n" +
                    $"Vehículo: {ingreso.Vehiculo?.Patente}\n" +
                    $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";

                await _notificacionService.CrearNotificacion(
                    usuarioId: mecanico.Id,
                    titulo: "Nueva Tarea Asignada",
                    mensaje: mensajeWhatsApp,
                    tipo: tipoNotificacion,
                    tareaId: tarea.Id,
                    ingresoId: ingresoId,
                    vehiculoId: ingreso.VehiculoId,
                    urlAccion: $"/Mecanico/DetalleTarea/{tarea.Id}"
                );

                Console.WriteLine($"[NOTIFICACION] Notificación creada exitosamente");

                if (tipoNotificacion == "Urgente" || tipoNotificacion == "Tarea")
                {
                    Console.WriteLine($"[WHATSAPP] Se enviará WhatsApp a: {mecanico.Telefono}");
                }
            }
            else
            {
                Console.WriteLine($"[ERROR] No se pudo crear notificación - Mecanico: {mecanico != null}, Ingreso: {ingreso != null}");
            }

            TempData["Mensaje"] = "Tarea asignada exitosamente. Se ha notificado al mecánico.";
            return RedirectToAction("GestionarIngreso", new { id = ingresoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPausa(int ingresoId, GestionarIngresoViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PausaMotivo))
            {
                TempData["Error"] = "Debe especificar el motivo de la pausa";
                return RedirectToAction("GestionarIngreso", new { id = ingresoId });
            }

            var pausa = new Pausa
            {
                IngresoTallerId = ingresoId,
                Motivo = model.PausaMotivo,
                Descripcion = model.PausaDescripcion ?? "Sin descripción",
                FechaInicio = DateTime.Now
            };

            _context.Pausas.Add(pausa);

            var ingreso = await _context.IngresosTaller.FindAsync(ingresoId);
            if (ingreso != null)
            {
                ingreso.Estado = "Pausado";
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Pausa registrada correctamente";
            return RedirectToAction("GestionarIngreso", new { id = ingresoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReanudarIngreso(int id)
        {
            var ingreso = await _context.IngresosTaller.FindAsync(id);
            if (ingreso == null)
            {
                return NotFound();
            }

            var pausaActiva = await _context.Pausas
            .Where(p => p.IngresoTallerId == id && p.FechaFin == null)
         .FirstOrDefaultAsync();

            if (pausaActiva != null)
            {
                pausaActiva.FechaFin = DateTime.Now;
            }

            ingreso.Estado = "En Proceso";
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Ingreso reanudado correctamente";
            return RedirectToAction("GestionarIngreso", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarIngreso(int id, GestionarIngresoViewModel model)
        {
            var ingreso = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
            {
                return NotFound();
            }

            // Verificar que todas las tareas estén completadas
            var tareasIncompletas = await _context.TareasTaller
                .Where(t => t.IngresoTallerId == id && t.Estado != "Completada")
                .CountAsync();

            if (tareasIncompletas > 0)
            {
                TempData["Error"] = $"Hay {tareasIncompletas} tarea(s) pendiente(s). Complete todas las tareas antes de finalizar.";
                return RedirectToAction("GestionarIngreso", new { id });
            }

            // IMPORTANTE: Solo cambiar el estado a Completado
            // NO establecer FechaSalidaReal aquí - eso lo hace el guardia cuando el vehículo sale físicamente
            ingreso.Estado = "Completado";

            if (!string.IsNullOrEmpty(model.ObservacionesFinales))
            {
                ingreso.DescripcionProblema += "\n\nObservaciones finales: " + model.ObservacionesFinales;
            }

            // El vehículo sigue en taller hasta que el guardia registre la salida
            // No cambiamos el estado del vehículo aquí

            await _context.SaveChangesAsync();

            // Notificar al chofer que el trabajo está completo pero el vehículo aún está en instalación
            await _notificacionService.CrearNotificacion(
                usuarioId: ingreso.ChoferId,
                titulo: "Trabajo completado",
                mensaje: $"El trabajo en su vehículo {ingreso.Vehiculo?.Patente} ha sido completado. El vehículo está listo, espere a que el guardia registre la salida.",
                tipo: "Info",
                ingresoId: ingreso.Id,
                vehiculoId: ingreso.VehiculoId,
                urlAccion: $"/Chofer/DetalleIngreso/{ingreso.Id}"
            );

            TempData["Mensaje"] = "Trabajo finalizado exitosamente. El vehículo permanece en instalación hasta que el guardia registre la salida.";
            return RedirectToAction("MonitoreoGeneral");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarMecanicoIngreso(int id, int mecanicoId)
        {
            var ingreso = await _context.IngresosTaller.FindAsync(id);
            if (ingreso == null)
            {
                return NotFound();
            }

            var mecanico = await _context.Usuarios.FindAsync(mecanicoId);
            if (mecanico == null || mecanico.Rol != "Mecanico")
            {
                TempData["Error"] = "Mecánico no válido";
                return RedirectToAction("GestionarIngreso", new { id });
            }

            ingreso.MecanicoAsignadoId = mecanicoId;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Mecánico {mecanico.Nombre} {mecanico.Apellido} asignado correctamente";
            return RedirectToAction("GestionarIngreso", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarIngreso(int id, string motivo)
        {
            var ingreso = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
            {
                return NotFound();
            }

            ingreso.Estado = "Cancelado";
            ingreso.DescripcionProblema += $"\n\nCancelado: {motivo}";

            if (ingreso.Vehiculo != null)
            {
                ingreso.Vehiculo.Estado = "Disponible";
                ingreso.Vehiculo.FechaActualizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Ingreso cancelado";
            return RedirectToAction("MonitoreoGeneral");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirDocumento(int id, IFormFile archivo, string tipoDocumento, string? descripcion)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar un archivo";
                return RedirectToAction("GestionarIngreso", new { id });
            }

            // Validar tamaño (10 MB máximo)
            if (archivo.Length > 10 * 1024 * 1024)
            {
                TempData["Error"] = "El archivo no puede superar los 10 MB";
                return RedirectToAction("GestionarIngreso", new { id });
            }

            // Validar extensión
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                TempData["Error"] = "Tipo de archivo no permitido";
                return RedirectToAction("GestionarIngreso", new { id });
            }

            try
            {
                // Crear carpeta si no existe
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documentos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generar nombre único para el archivo
                var nombreUnico = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(uploadsFolder, nombreUnico);

                // Guardar el archivo
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // Crear registro en BD
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var documento = new Documento
                {
                    IngresoTallerId = id,
                    TipoDocumento = tipoDocumento,
                    NombreArchivo = archivo.FileName,
                    RutaArchivo = $"/uploads/documentos/{nombreUnico}",
                    FechaSubida = DateTime.Now,
                    UsuarioSubidaId = usuarioId,
                    Descripcion = descripcion,
                    TamañoBytes = archivo.Length
                };

                _context.Documentos.Add(documento);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"Documento '{archivo.FileName}' subido exitosamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al subir el documento: {ex.Message}";
            }

            return RedirectToAction("GestionarIngreso", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> DescargarDocumento(int id)
        {
            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
            {
                return NotFound();
            }

            var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", documento.RutaArchivo.TrimStart('/'));

            if (!System.IO.File.Exists(rutaCompleta))
            {
                TempData["Error"] = "El archivo no existe en el servidor";
                return RedirectToAction("GestionarIngreso", new { id = documento.IngresoTallerId });
            }

            var memoria = new MemoryStream();
            using (var stream = new FileStream(rutaCompleta, FileMode.Open))
            {
                await stream.CopyToAsync(memoria);
            }
            memoria.Position = 0;

            var extension = Path.GetExtension(documento.NombreArchivo).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            return File(memoria, contentType, documento.NombreArchivo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarDocumento(int id)
        {
            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
            {
                return NotFound();
            }

            var ingresoId = documento.IngresoTallerId;

            try
            {
                // Eliminar archivo físico
                var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", documento.RutaArchivo.TrimStart('/'));
                if (System.IO.File.Exists(rutaCompleta))
                {
                    System.IO.File.Delete(rutaCompleta);
                }

                // Eliminar registro de BD
                _context.Documentos.Remove(documento);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Documento eliminado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar el documento: {ex.Message}";
            }

            return RedirectToAction("GestionarIngreso", new { id = ingresoId });
        }
    }
}
