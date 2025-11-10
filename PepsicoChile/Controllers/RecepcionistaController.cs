using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepsicoChile.Data;
using PepsicoChile.Filters;
using PepsicoChile.Models;
using PepsicoChile.Models.ViewModels;

namespace PepsicoChile.Controllers
{
    [AuthorizeSession]
    [AuthorizeRole("RecepcionistaVehiculos", "Administrador", "Supervisor")]
    public class RecepcionistaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecepcionistaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Gestión de Vehículos
        public async Task<IActionResult> GestionVehiculos()
        {
            var ingresos = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.MecanicoAsignado)
                .Where(i => i.Estado != "Completado" && i.Estado != "Cancelado")
                .OrderByDescending(i => i.FechaIngresoReal ?? i.FechaProgramada)
                .ToListAsync();

            return View(ingresos);
        }

        // GET: Registrar Patente / Nuevo Vehículo
        [HttpGet]
        public async Task<IActionResult> RegistrarPatente()
        {
            var model = new RegistroIngresoViewModel
            {
                FechaProgramada = DateTime.Now,
                NumeroOT = $"OT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}"
            };

            // Cargar datos para dropdowns
            model.VehiculosDisponibles = await _context.Vehiculos
                .Where(v => v.Estado == "Activo")
                .OrderBy(v => v.Patente)
                .ToListAsync();

            model.ChofersDisponibles = await _context.Usuarios
                .Where(u => u.Rol == "Chofer" && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(model);
        }

        // POST: Registrar Patente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPatente(RegistroIngresoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.VehiculosDisponibles = await _context.Vehiculos
                    .Where(v => v.Estado == "Activo")
                    .OrderBy(v => v.Patente)
                    .ToListAsync();

                model.ChofersDisponibles = await _context.Usuarios
                    .Where(u => u.Rol == "Chofer" && u.Activo)
                    .OrderBy(u => u.Nombre)
                    .ToListAsync();

                return View(model);
            }

            var recepcionistaId = HttpContext.Session.GetInt32("UsuarioId");

            // Crear nuevo ingreso
            var ingreso = new IngresoTaller
            {
                VehiculoId = model.VehiculoId,
                ChoferId = model.ChoferId,
                FechaProgramada = model.FechaProgramada,
                FechaIngresoReal = DateTime.Now,
                MotivoIngreso = model.MotivoIngreso,
                DescripcionProblema = model.DescripcionProblema,
                ObservacionesChofer = model.ObservacionesChofer,
                Estado = "En Proceso",
                KilometrajeIngreso = model.KilometrajeIngreso,
                RequiereRepuestos = model.RequiereRepuestos,
                NumeroOT = model.NumeroOT,
                SupervisorId = recepcionistaId // Recepcionista actúa como responsable del registro
            };

            _context.IngresosTaller.Add(ingreso);
            await _context.SaveChangesAsync();

            // Guardar imágenes si se adjuntaron
            if (model.ImagenesVehiculo != null && model.ImagenesVehiculo.Any())
            {
                await GuardarImagenesIngreso(ingreso.Id, model.ImagenesVehiculo, recepcionistaId);
            }

            TempData["Mensaje"] = $"Vehículo {ingreso.NumeroOT} registrado exitosamente";
            return RedirectToAction("DetalleIngreso", new { id = ingreso.Id });
        }

        // GET: Detalle de Ingreso
        public async Task<IActionResult> DetalleIngreso(int id)
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
                return RedirectToAction("GestionVehiculos");
            }

            // Obtener documentos
            var documentos = await _context.Documentos
                .Where(d => d.IngresoTallerId == id)
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();

            // Obtener imágenes
            var imagenes = await _context.ImagenesIngreso
                .Include(img => img.UsuarioSubida)
                .Where(img => img.IngresoTallerId == id)
                .OrderByDescending(img => img.FechaSubida)
                .ToListAsync();

            // Obtener tareas
            var tareas = await _context.TareasTaller
                .Include(t => t.MecanicoAsignado)
                .Where(t => t.IngresoTallerId == id)
                .OrderBy(t => t.Prioridad)
                .ThenBy(t => t.FechaAsignacion)
                .ToListAsync();

            ViewBag.Documentos = documentos;
            ViewBag.Imagenes = imagenes;
            ViewBag.Tareas = tareas;

            return View(ingreso);
        }

        // GET: Gestionar Documentos
        public async Task<IActionResult> GestionarDocumentos(int id)
        {
            var ingreso = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
            {
                TempData["Error"] = "Ingreso no encontrado";
                return RedirectToAction("GestionVehiculos");
            }

            var documentos = await _context.Documentos
                .Include(d => d.UsuarioSubida)
                .Where(d => d.IngresoTallerId == id)
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();

            ViewBag.Ingreso = ingreso;
            return View(documentos);
        }

        // POST: Subir Documento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirDocumento(int ingresoId, string tipoDocumento, string descripcion, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar un archivo";
                return RedirectToAction("GestionarDocumentos", new { id = ingresoId });
            }

            var recepcionistaId = HttpContext.Session.GetInt32("UsuarioId");
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documentos");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var nombreArchivo = $"doc_{ingresoId}_{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);
            var rutaRelativa = Path.Combine("uploads", "documentos", nombreArchivo).Replace("\\", "/");

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var documento = new Documento
            {
                IngresoTallerId = ingresoId,
                TipoDocumento = tipoDocumento,
                Descripcion = descripcion,
                NombreArchivo = archivo.FileName,
                RutaArchivo = rutaRelativa,
                FechaSubida = DateTime.Now,
                TamañoBytes = archivo.Length,
                UsuarioSubidaId = recepcionistaId,
                EstadoValidacion = "Pendiente"
            };

            _context.Documentos.Add(documento);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Documento subido exitosamente";
            return RedirectToAction("GestionarDocumentos", new { id = ingresoId });
        }

        // POST: Validar Documento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidarDocumento(int id, string estado, string observaciones)
        {
            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
            {
                TempData["Error"] = "Documento no encontrado";
                return RedirectToAction("GestionVehiculos");
            }

            documento.EstadoValidacion = estado; // "Aprobado" o "Rechazado"
            documento.ObservacionesValidacion = observaciones;
            documento.FechaValidacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Documento {estado.ToLower()} exitosamente";
            return RedirectToAction("GestionarDocumentos", new { id = documento.IngresoTallerId });
        }

        // GET: Documentación Técnica
        public async Task<IActionResult> DocumentacionTecnica()
        {
            var documentos = await _context.Documentos
                .Include(d => d.IngresoTaller)
                    .ThenInclude(i => i.Vehiculo)
                .Include(d => d.UsuarioSubida)
                .Where(d => d.EstadoValidacion == "Pendiente")
                .OrderByDescending(d => d.FechaSubida)
                .ToListAsync();

            return View(documentos);
        }

        // GET: Historial de OTs
        public async Task<IActionResult> HistorialOTs(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? DateTime.Today.AddDays(-30);
            var fin = fechaFin ?? DateTime.Today.AddDays(1);

            var ingresos = await _context.IngresosTaller
                .Include(i => i.Vehiculo)
                .Include(i => i.Chofer)
                .Include(i => i.MecanicoAsignado)
                .Where(i => !string.IsNullOrEmpty(i.NumeroOT) &&
                           i.FechaIngresoReal >= inicio &&
                           i.FechaIngresoReal < fin)
                .OrderByDescending(i => i.FechaIngresoReal)
                .ToListAsync();

            ViewBag.FechaInicio = inicio;
            ViewBag.FechaFin = fin.AddDays(-1);

            return View(ingresos);
        }

        // Método auxiliar para guardar imágenes
        private async Task GuardarImagenesIngreso(int ingresoId, List<IFormFile> imagenes, int? usuarioId)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ingresos");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            foreach (var imagen in imagenes)
            {
                if (imagen.Length > 0)
                {
                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        continue;
                    }

                    var nombreArchivo = $"ingreso_{ingresoId}_{Guid.NewGuid()}{extension}";
                    var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);
                    var rutaRelativa = Path.Combine("uploads", "ingresos", nombreArchivo).Replace("\\", "/");

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }

                    var imagenIngreso = new ImagenIngreso
                    {
                        IngresoTallerId = ingresoId,
                        TipoImagen = "Ingreso",
                        NombreArchivo = imagen.FileName,
                        RutaArchivo = rutaRelativa,
                        FechaSubida = DateTime.Now,
                        TamañoBytes = imagen.Length,
                        UsuarioSubidaId = usuarioId,
                        Descripcion = "Imagen subida por recepcionista"
                    };

                    _context.ImagenesIngreso.Add(imagenIngreso);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
