using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;
using PepsicoChile.Filters;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("GuardiaAcceso", "Administrador", "Supervisor")]
    public class GuardiaAccesoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuardiaAccesoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Agenda(DateTime? fecha)
        {
            // Si no se proporciona fecha, usar hoy
            var fechaSeleccionada = fecha ?? DateTime.Today;
            
            // Obtener ingresos programados para el rango de 7 días (fecha ± 3 días)
            var fechaInicio = fechaSeleccionada.AddDays(-3);
            var fechaFin = fechaSeleccionada.AddDays(4);

            var ingresos = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Where(i => i.FechaProgramada >= fechaInicio && i.FechaProgramada < fechaFin)
                .OrderBy(i => i.FechaProgramada)
                .ToListAsync();

            // Estadísticas del día seleccionado
            var ingresosDia = ingresos.Where(i => i.FechaProgramada.Date == fechaSeleccionada.Date).ToList();
            
            ViewBag.FechaSeleccionada = fechaSeleccionada;
            ViewBag.TotalProgramados = ingresosDia.Count;
            ViewBag.Completados = ingresosDia.Count(i => i.FechaIngresoReal.HasValue);
            ViewBag.Pendientes = ingresosDia.Count(i => !i.FechaIngresoReal.HasValue);
            ViewBag.Retrasados = ingresosDia.Count(i => !i.FechaIngresoReal.HasValue && i.FechaProgramada < DateTime.Now);

            return View(ingresos);
        }

        [HttpGet]
        public async Task<IActionResult> RegistrarLlegada(int? id)
        {
            var model = new RegistroLlegadaViewModel
            {
                FechaHoraLlegada = DateTime.Now
            };

            // Obtener lista de vehículos para el dropdown
            model.VehiculosDisponibles = await _context.Vehiculos
                .Where(v => v.Estado == "Activo")
                .OrderBy(v => v.Patente)
                .ToListAsync();

            // Si se proporciona un ID, cargar datos del ingreso programado
            if (id.HasValue)
            {
                var ingreso = await _context.IngresosTaller
                    .Include(i => i.Vehiculo)
                    .Include(i => i.Chofer)
                    .FirstOrDefaultAsync(i => i.Id == id.Value);

                if (ingreso != null)
                {
                    model.VehiculoId = ingreso.VehiculoId;
                    model.Patente = ingreso.Vehiculo?.Patente ?? "";
                    model.ChoferId = ingreso.ChoferId;
                    model.NombreChofer = $"{ingreso.Chofer?.Nombre} {ingreso.Chofer?.Apellido}";
                    model.MotivoIngreso = ingreso.MotivoIngreso;
                    model.DescripcionProblema = ingreso.DescripcionProblema ?? "";
                    model.ObservacionesChofer = ingreso.ObservacionesChofer;
                    model.KilometrajeIngreso = ingreso.KilometrajeIngreso ?? 0;
                    model.RequiereRepuestos = ingreso.RequiereRepuestos;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarLlegada(int? id, RegistroLlegadaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.VehiculosDisponibles = await _context.Vehiculos
                    .Where(v => v.Estado == "Activo")
                    .OrderBy(v => v.Patente)
                    .ToListAsync();
                return View(model);
            }

            var guardiaId = HttpContext.Session.GetInt32("UsuarioId");

            IngresoTaller? ingreso = null;

            // Si hay un ID, es un ingreso programado
            if (id.HasValue)
            {
                ingreso = await _context.IngresosTaller.FindAsync(id.Value);
                if (ingreso != null)
                {
                    ingreso.FechaIngresoReal = model.FechaHoraLlegada;
                    ingreso.Estado = "En Proceso";
                    ingreso.KilometrajeIngreso = model.KilometrajeIngreso;
                    
                    // Actualizar observaciones si se proporcionaron
                    if (!string.IsNullOrEmpty(model.ObservacionesChofer))
                    {
                        ingreso.ObservacionesChofer = string.IsNullOrEmpty(ingreso.ObservacionesChofer)
                            ? $"[Guardia - {DateTime.Now:dd/MM/yyyy HH:mm}] {model.ObservacionesChofer}"
                            : ingreso.ObservacionesChofer + $"\n[Guardia - {DateTime.Now:dd/MM/yyyy HH:mm}] {model.ObservacionesChofer}";
                    }
                }
            }
            else
            {
                // Nuevo ingreso no programado
                ingreso = new IngresoTaller
                {
                    VehiculoId = model.VehiculoId,
                    ChoferId = model.ChoferId,
                    FechaProgramada = model.FechaHoraLlegada,
                    FechaIngresoReal = model.FechaHoraLlegada,
                    MotivoIngreso = model.MotivoIngreso,
                    DescripcionProblema = model.DescripcionProblema,
                    ObservacionesChofer = $"[Guardia - {DateTime.Now:dd/MM/yyyy HH:mm}] {model.ObservacionesChofer}",
                    Estado = "En Proceso",
                    KilometrajeIngreso = model.KilometrajeIngreso,
                    RequiereRepuestos = model.RequiereRepuestos
                };

                _context.IngresosTaller.Add(ingreso);
            }

            await _context.SaveChangesAsync();

            // Guardar imágenes si se adjuntaron
            if (model.ImagenesVehiculo != null && model.ImagenesVehiculo.Any())
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "llegadas");
                
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                foreach (var imagen in model.ImagenesVehiculo)
                {
                    if (imagen.Length > 0)
                    {
                        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
                        
                        if (!extensionesPermitidas.Contains(extension))
                        {
                            continue;
                        }

                        var nombreArchivo = $"llegada_{ingreso.Id}_{Guid.NewGuid()}{extension}";
                        var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);
                        var rutaRelativa = Path.Combine("uploads", "llegadas", nombreArchivo).Replace("\\", "/");

                        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            await imagen.CopyToAsync(stream);
                        }

                        var imagenIngreso = new ImagenIngreso
                        {
                            IngresoTallerId = ingreso.Id,
                            TipoImagen = "Llegada",
                            NombreArchivo = imagen.FileName,
                            RutaArchivo = rutaRelativa,
                            FechaSubida = DateTime.Now,
                            TamañoBytes = imagen.Length,
                            UsuarioSubidaId = guardiaId,
                            Descripcion = $"Imagen de llegada registrada por guardia"
                        };

                        _context.ImagenesIngreso.Add(imagenIngreso);
                    }
                }

                await _context.SaveChangesAsync();
            }

            TempData["Mensaje"] = id.HasValue 
                ? "Llegada registrada exitosamente para el ingreso programado"
                : "Nuevo ingreso registrado exitosamente";

            return RedirectToAction("Agenda");
        }

        public async Task<IActionResult> HistorialLlegadas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Rango por defecto: últimos 7 días
            var inicio = fechaInicio ?? DateTime.Today.AddDays(-7);
            var fin = fechaFin ?? DateTime.Today.AddDays(1);

            var llegadas = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Where(i => i.FechaIngresoReal.HasValue && 
                           i.FechaIngresoReal.Value >= inicio && 
                           i.FechaIngresoReal.Value < fin)
                .OrderByDescending(i => i.FechaIngresoReal)
                .ToListAsync();

            ViewBag.FechaInicio = inicio;
            ViewBag.FechaFin = fin.AddDays(-1);
            ViewBag.TotalLlegadas = llegadas.Count;

            return View(llegadas);
        }

        public async Task<IActionResult> DetalleIngreso(int id)
        {
            var ingreso = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Include(i => i.MecanicoAsignado)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
            {
                TempData["Error"] = "Ingreso no encontrado";
                return RedirectToAction("Agenda");
            }

            // Obtener imágenes del ingreso
            var imagenes = await _context.ImagenesIngreso
                .Where(img => img.IngresoTallerId == id)
                .OrderBy(img => img.FechaSubida)
                .ToListAsync();

            ViewBag.Imagenes = imagenes;

            return View(ingreso);
        }

        [HttpGet]
        public async Task<IActionResult> VehiculosEnTaller()
        {
            // Obtener TODOS los vehículos que están físicamente en la instalación 
            // (han ingresado pero no han salido, sin importar el estado del trabajo)
            var vehiculosEnTaller = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.MecanicoAsignado)
                .Include(i => i.Supervisor)
                .Where(i => i.FechaIngresoReal.HasValue && !i.FechaSalidaReal.HasValue)
                .OrderByDescending(i => i.Estado == "Completado") // Listos para salir primero
                .ThenByDescending(i => i.FechaIngresoReal)
                .ToListAsync();

            // Estadísticas
            ViewBag.TotalEnTaller = vehiculosEnTaller.Count;
            ViewBag.ListosParaSalir = vehiculosEnTaller.Count(v => v.Estado == "Completado");
            ViewBag.EnProceso = vehiculosEnTaller.Count(v => v.Estado == "En Proceso");
            ViewBag.Pausados = vehiculosEnTaller.Count(v => v.Estado == "Pausado");

            return View(vehiculosEnTaller);
        }

        [HttpGet]
        public async Task<IActionResult> RegistrarSalida(int id)
        {
            var ingreso = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.MecanicoAsignado)
                .Include(i => i.Supervisor)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
            {
                TempData["Error"] = "Ingreso no encontrado";
                return RedirectToAction("VehiculosEnTaller");
            }

            if (!ingreso.FechaIngresoReal.HasValue)
            {
                TempData["Error"] = "El vehículo aún no ha ingresado a la instalación";
                return RedirectToAction("Agenda");
            }

            if (ingreso.FechaSalidaReal.HasValue)
            {
                TempData["Error"] = "Este vehículo ya salió de la instalación";
                return RedirectToAction("HistorialSalidas");
            }

            // VALIDACIÓN IMPORTANTE: Solo puede salir si el trabajo está completado
            if (ingreso.Estado != "Completado")
            {
                TempData["Error"] = $"No se puede registrar la salida. El trabajo está en estado: {ingreso.Estado}. Debe estar Completado.";
                return RedirectToAction("VehiculosEnTaller");
            }

            // Obtener tareas del ingreso para mostrar información
            var tareas = await _context.TareasTaller
                .Include(t => t.MecanicoAsignado)
                .Where(t => t.IngresoTallerId == id)
                .ToListAsync();

            ViewBag.Tareas = tareas;

            return View(ingreso);
        }

        [HttpGet]
        public async Task<IActionResult> RegistrarSalidaNuevo()
        {
            // Obtener todos los vehículos que están físicamente en la instalación
            // (han ingresado pero no han salido)
            var ingresosDisponibles = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Where(i => i.FechaIngresoReal.HasValue && !i.FechaSalidaReal.HasValue)
                .OrderByDescending(i => i.Estado == "Completado") // Completados primero
                .ThenBy(i => i.FechaIngresoReal)
                .ToListAsync();

            var model = new RegistroSalidaViewModel
            {
                IngresosDisponibles = ingresosDisponibles.Select(i => new IngresoDisponibleSalida
                {
                    Id = i.Id,
                    Patente = i.Vehiculo?.Patente ?? "",
                    VehiculoInfo = $"{i.Vehiculo?.Marca} {i.Vehiculo?.Modelo} ({i.Vehiculo?.Año})",
                    ChoferNombre = $"{i.Chofer?.Nombre} {i.Chofer?.Apellido}",
                    FechaLlegada = i.FechaIngresoReal!.Value,
                    KilometrajeIngreso = i.KilometrajeIngreso ?? 0,
                    Estado = i.Estado,
                    EstadoColor = i.Estado == "Completado" ? "success" : 
                                  i.Estado == "En Proceso" ? "warning" : 
                                  i.Estado == "Pausado" ? "danger" : "info",
                    NumeroOT = i.NumeroOT ?? "Sin OT",
                    TiempoEnTaller = (int)(DateTime.Now - i.FechaIngresoReal.Value).TotalHours
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSalidaNuevo(RegistroSalidaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar la lista de ingresos disponibles
                var ingresosDisponibles = await _context.IngresosTaller
                    .Include(i => i.Vehiculo)
                    .Include(i => i.Chofer)
                    .Where(i => i.FechaIngresoReal.HasValue && !i.FechaSalidaReal.HasValue)
                    .OrderByDescending(i => i.Estado == "Completado")
                    .ThenBy(i => i.FechaIngresoReal)
                    .ToListAsync();

                model.IngresosDisponibles = ingresosDisponibles.Select(i => new IngresoDisponibleSalida
                {
                    Id = i.Id,
                    Patente = i.Vehiculo?.Patente ?? "",
                    VehiculoInfo = $"{i.Vehiculo?.Marca} {i.Vehiculo?.Modelo} ({i.Vehiculo?.Año})",
                    ChoferNombre = $"{i.Chofer?.Nombre} {i.Chofer?.Apellido}",
                    FechaLlegada = i.FechaIngresoReal!.Value,
                    KilometrajeIngreso = i.KilometrajeIngreso ?? 0,
                    Estado = i.Estado,
                    EstadoColor = i.Estado == "Completado" ? "success" : 
                                  i.Estado == "En Proceso" ? "warning" : 
                                  i.Estado == "Pausado" ? "danger" : "info",
                    NumeroOT = i.NumeroOT ?? "Sin OT",
                    TiempoEnTaller = (int)(DateTime.Now - i.FechaIngresoReal.Value).TotalHours
                }).ToList();

                return View(model);
            }

            var ingreso = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(i => i.Id == model.IngresoId);

            if (ingreso == null)
            {
                TempData["Error"] = "Ingreso no encontrado";
                return RedirectToAction("VehiculosEnTaller");
            }

            if (ingreso.FechaSalidaReal.HasValue)
            {
                TempData["Error"] = "Este vehículo ya salió de la instalación";
                return RedirectToAction("HistorialSalidas");
            }

            // Validar que el trabajo esté completado
            if (ingreso.Estado != "Completado")
            {
                TempData["Error"] = $"No se puede registrar la salida. El trabajo está en estado: {ingreso.Estado}. Debe estar Completado primero.";
                return RedirectToAction("RegistrarSalidaNuevo");
            }

            // Validar kilometraje
            if (ingreso.KilometrajeIngreso.HasValue && model.KilometrajeSalida < ingreso.KilometrajeIngreso.Value)
            {
                TempData["Error"] = $"El kilometraje de salida ({model.KilometrajeSalida} km) no puede ser menor al kilometraje de ingreso ({ingreso.KilometrajeIngreso.Value} km)";
                return RedirectToAction("RegistrarSalidaNuevo");
            }

            var guardiaId = HttpContext.Session.GetInt32("UsuarioId");

            // Registrar salida
            ingreso.FechaSalidaReal = DateTime.Now;
            ingreso.KilometrajeSalida = model.KilometrajeSalida;

            // Cambiar estado del vehículo a Disponible
            if (ingreso.Vehiculo != null)
            {
                ingreso.Vehiculo.Estado = "Disponible";
                ingreso.Vehiculo.KilometrajeActual = model.KilometrajeSalida;
                ingreso.Vehiculo.FechaActualizacion = DateTime.Now;
            }

            // Agregar observaciones de salida
            if (!string.IsNullOrEmpty(model.ObservacionesSalida))
            {
                var observacionCompleta = $"[SALIDA - Guardia - {DateTime.Now:dd/MM/yyyy HH:mm}] Km: {model.KilometrajeSalida} - {model.ObservacionesSalida}";

                ingreso.ObservacionesChofer = string.IsNullOrEmpty(ingreso.ObservacionesChofer)
                    ? observacionCompleta
                    : ingreso.ObservacionesChofer + "\n\n" + observacionCompleta;
            }

            await _context.SaveChangesAsync();

            // Guardar imágenes de salida si se adjuntaron
            if (model.ImagenesSalida != null && model.ImagenesSalida.Any())
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "salidas");

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                foreach (var imagen in model.ImagenesSalida)
                {
                    if (imagen.Length > 0)
                    {
                        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

                        if (!extensionesPermitidas.Contains(extension))
                        {
                            continue;
                        }

                        var nombreArchivo = $"salida_{model.IngresoId}_{Guid.NewGuid()}{extension}";
                        var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);
                        var rutaRelativa = Path.Combine("uploads", "salidas", nombreArchivo).Replace("\\", "/");

                        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            await imagen.CopyToAsync(stream);
                        }

                        var imagenIngreso = new ImagenIngreso
                        {
                            IngresoTallerId = model.IngresoId,
                            TipoImagen = "Salida",
                            NombreArchivo = imagen.FileName,
                            RutaArchivo = rutaRelativa,
                            FechaSubida = DateTime.Now,
                            TamañoBytes = imagen.Length,
                            UsuarioSubidaId = guardiaId,
                            Descripcion = $"Imagen de salida - Km: {model.KilometrajeSalida}"
                        };

                        _context.ImagenesIngreso.Add(imagenIngreso);
                    }
                }

                await _context.SaveChangesAsync();
            }

            // Enviar notificación al chofer
            var notificacion = new Notificacion
            {
                UsuarioId = ingreso.ChoferId,
                Titulo = "Vehículo disponible para retiro",
                Mensaje = $"Su vehículo {ingreso.Vehiculo?.Patente} ha salido de la instalación del taller. Kilometraje de salida: {model.KilometrajeSalida} km. El trabajo ha sido completado.",
                Tipo = "Info",
                IngresoId = ingreso.Id,
                VehiculoId = ingreso.VehiculoId,
                UrlAccion = $"/Chofer/DetalleIngreso/{ingreso.Id}",
                FechaCreacion = DateTime.Now,
                Leida = false
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = model.ImagenesSalida != null && model.ImagenesSalida.Any()
                ? $"Salida registrada exitosamente con {model.ImagenesSalida.Count} imagen(es). El chofer ha sido notificado."
                : "Salida de la instalación registrada exitosamente. El chofer ha sido notificado.";

            return RedirectToAction("VehiculosEnTaller");
        }

        public async Task<IActionResult> HistorialSalidas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Rango por defecto: últimos 7 días
            var inicio = fechaInicio ?? DateTime.Today.AddDays(-7);
            var fin = fechaFin ?? DateTime.Today.AddDays(1);

            var salidas = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.Supervisor)
                .Include(i => i.MecanicoAsignado)
                .Where(i => i.FechaSalidaReal.HasValue && 
                           i.FechaSalidaReal.Value >= inicio && 
                           i.FechaSalidaReal.Value < fin)
                .OrderByDescending(i => i.FechaSalidaReal)
                .ToListAsync();

            ViewBag.FechaInicio = inicio;
            ViewBag.FechaFin = fin.AddDays(-1);
            ViewBag.TotalSalidas = salidas.Count;

            return View(salidas);
        }
    }
}
